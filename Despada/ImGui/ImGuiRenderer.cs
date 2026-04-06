using System.Collections.Concurrent;
using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices;
using HarmonyLib;
using ImGuiNET;

public record struct Toast(string Title, string Body, float TimeLeft);

public static class ImGuiRenderer
{
    private static bool _initialized;
    private static bool _initFailed;

    private static int _screenW;
    private static int _screenH;

    private static int   _stableFrames;
    private static int   _lastSeenW;
    private static int   _lastSeenH;
    private const  int   StableFramesRequired = 30;
    
    public static volatile bool OverlayVisible = false;
    private static bool _prevOverlayVisible = false;

    public static bool ShowDemoWindow = true;
    
    private static readonly System.Diagnostics.Stopwatch _stopwatch = System.Diagnostics.Stopwatch.StartNew();
    
    private static readonly ConcurrentQueue<Toast> _pendingToasts = new();
    private static readonly List<Toast>            _activeToasts  = new();

    public static void ShowToast(string title, string body, float seconds = 3f)
        => _pendingToasts.Enqueue(new Toast(title, body, seconds));

    public static void RenderFrame()
    {
        if (_initFailed) return;

        if (OverlayVisible != _prevOverlayVisible)
        {
            _prevOverlayVisible = OverlayVisible;
            SetCursorVisible(OverlayVisible);

            if (OverlayVisible)
                TryStartTextInput();
        }

        bool hasToasts = !_pendingToasts.IsEmpty || _activeToasts.Count > 0;
        bool needRender = OverlayVisible || hasToasts;

        if (!needRender)
        {
            ImGuiInputHook.FlushEvents();
            return;
        }

        try
        {
            var (w, h) = GlStateGuard.GetFramebufferSize();

            if (w > 64 && h > 64)
            {
                if (w == _lastSeenW && h == _lastSeenH)
                    _stableFrames++;
                else
                {
                    _lastSeenW    = w;
                    _lastSeenH    = h;
                    _stableFrames = 0;
                }
                _screenW = w;
                _screenH = h;
            }

            if (!_initialized)
            {
                if (_stableFrames < StableFramesRequired) return;
                Initialize();
            }
            if (!_initialized) return;

            var elapsedSec = (float)_stopwatch.Elapsed.TotalSeconds;
            _stopwatch.Restart();

            var io = ImGui.GetIO();
            io.DisplaySize             = new Vector2(_screenW, _screenH);
            io.DisplayFramebufferScale = new Vector2(1f, 1f);
            io.DeltaTime               = Math.Clamp(elapsedSec, 0.0001f, 0.1f);

            ImGuiInputHook.FlushEvents();

            GlStateGuard.Save();
            try
            {
                ImGui.NewFrame();

                ImGuiInputHook.SnapshotWantCapture();

                UpdateSdlTextInput(io.WantTextInput);

                DrawToasts(elapsedSec);

                if (OverlayVisible)
                    DrawUI();

                ImGui.Render();
                RenderDrawData(ImGui.GetDrawData());
            }
            finally
            {
                GlStateGuard.Restore();
            }
        }
        catch (Exception ex)
        {
            MarseyLogger.Fatal($"[ImGuiRenderer] RenderFrame: {ex}");
            _initFailed = true;
        }
    }

    private static bool _sdlTextInputActive = false;

    private static void UpdateSdlTextInput(bool wantTextInput)
    {
        if (wantTextInput == _sdlTextInputActive) return;
        _sdlTextInputActive = wantTextInput;

        if (wantTextInput)
            TryStartTextInput();
    }

    private static void TryStartTextInput()
    {
        try
        {
            var clyde     = ResolveFromIoC("Robust.Client.Graphics.IClyde");
            if (clyde is null) return;

            var clydeType = clyde.GetType();

            object? windowing = null;
            var     t         = clydeType;
            while (t is not null && windowing is null)
            {
                windowing = AccessTools.Field(t, "_windowing")?.GetValue(clyde);
                t         = t.BaseType;
            }
            if (windowing is null) return;

            object? mainWindow = null;
            t = clydeType;
            while (t is not null && mainWindow is null)
            {
                mainWindow = AccessTools.Field(t, "_mainWindow")?.GetValue(clyde)
                          ?? AccessTools.Field(t, "_mainReg")?.GetValue(clyde)
                          ?? AccessTools.Property(t, "MainWindow")?.GetValue(clyde);
                t = t.BaseType;
            }
            if (mainWindow is null) return;

            AccessTools.Method(windowing.GetType(), "TextInputStart")
                       ?.Invoke(windowing, [mainWindow]);

            MarseyLogger.Info("[ImGuiRenderer] SDL TextInputStart called.");
        }
        catch (Exception ex)
        {
            MarseyLogger.Warn($"[ImGuiRenderer] TryStartTextInput: {ex.Message}");
        }
    }

    private static void DrawToasts(float dt)
    {
        while (_pendingToasts.TryDequeue(out var t))
            _activeToasts.Add(t);

        for (int i = _activeToasts.Count - 1; i >= 0; i--)
        {
            var updated = _activeToasts[i] with { TimeLeft = _activeToasts[i].TimeLeft - dt };
            if (updated.TimeLeft <= 0f)
                _activeToasts.RemoveAt(i);
            else
                _activeToasts[i] = updated;
        }

        if (_activeToasts.Count == 0) return;

        const float PadX      = 14f;
        const float PadY      = 14f;
        const float Width     = 280f;
        const float Height    = 64f;
        const float FadeTime  = 0.6f;

        ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 7f);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 0f);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(10f, 6f));

        float y = _screenH - PadY;

        for (int i = 0; i < _activeToasts.Count; i++)
        {
            var toast = _activeToasts[i];
            y -= Height + PadY;

            float alpha = Math.Min(1f, toast.TimeLeft / FadeTime);

            ImGui.SetNextWindowPos(new Vector2(_screenW - Width - PadX, y), ImGuiCond.Always);
            ImGui.SetNextWindowSize(new Vector2(Width, Height), ImGuiCond.Always);
            ImGui.SetNextWindowBgAlpha(0.88f * alpha);

            var flags = ImGuiWindowFlags.NoDecoration
                      | ImGuiWindowFlags.NoInputs
                      | ImGuiWindowFlags.NoNav
                      | ImGuiWindowFlags.NoMove
                      | ImGuiWindowFlags.NoSavedSettings
                      | ImGuiWindowFlags.NoBringToFrontOnFocus;

            if (ImGui.Begin($"##toast_{i}", flags))
            {
                ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1f, 0.85f, 0.2f, alpha));
                ImGui.TextUnformatted(toast.Title);
                ImGui.PopStyleColor();

                ImGui.Separator();

                ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1f, 1f, 1f, alpha * 0.9f));
                ImGui.TextWrapped(toast.Body);
                ImGui.PopStyleColor();
            }
            ImGui.End();
        }

        ImGui.PopStyleVar(3);
    }

    private static void Initialize()
    {
        try
        {
            MarseyLogger.Info($"[ImGuiRenderer] Initializing ImGui at {_screenW}x{_screenH}...");
            NativeLoader.EnsureLoaded();

            ImGui.CreateContext();

            var io = ImGui.GetIO();
            io.DisplaySize             = new Vector2(_screenW, _screenH);
            io.DisplayFramebufferScale = new Vector2(1f, 1f);

            float scale = Math.Clamp(_screenH / 1080f, 0.75f, 4f);
            MarseyLogger.Info($"[ImGuiRenderer] DPI scale: {scale:F2}x (stable after {_stableFrames} frames)");

            LoadFont(io, scale);

            io.Fonts.Build();
            io.Fonts.GetTexDataAsRGBA32(out nint pixels, out int fw, out int fh, out _);
            MarseyLogger.Info($"[ImGuiRenderer] Font atlas: {fw}x{fh}");

            _fontTextureId = CreateFontTexture(pixels, fw, fh);
            io.Fonts.SetTexID(_fontTextureId);
            io.Fonts.ClearTexData();

            ImGui.GetStyle().ScaleAllSizes(scale);

            CreateGlObjects();

            MarseyLogger.Info("[ImGuiRenderer] ImGui initialized successfully.");
            _initialized = true;
        }
        catch (Exception ex)
        {
            MarseyLogger.Fatal($"[ImGuiRenderer] Initialization failed: {ex}");
            _initFailed = true;
        }
    }

    private static GCHandle _fontDataHandle;

    private static unsafe void LoadFont(ImGuiIOPtr io, float scale)
    {
        const string resourceName = "Despada.Assets.JetBrainsMonoNerdFontMono-Regular.ttf";
        var fontSize = MathF.Floor(16f * scale);

        var asm = Assembly.GetExecutingAssembly();
        using var stream = asm.GetManifestResourceStream(resourceName);
        if (stream is null)
        {
            MarseyLogger.Warn($"[ImGuiRenderer] Font not found: '{resourceName}'");
            io.Fonts.AddFontDefault();
            return;
        }

        var fontData = new byte[stream.Length];
        _ = stream.Read(fontData, 0, fontData.Length);

        _fontDataHandle = GCHandle.Alloc(fontData, GCHandleType.Pinned);

        var cfg = new ImFontConfigPtr(ImGuiNative.ImFontConfig_ImFontConfig());
        cfg.FontDataOwnedByAtlas = false;
        cfg.OversampleH          = 2;
        cfg.OversampleV          = 2;
        cfg.PixelSnapH           = true;

        var ranges       = BuildGlyphRanges();
        var rangesHandle = GCHandle.Alloc(ranges, GCHandleType.Pinned);

        io.Fonts.AddFontFromMemoryTTF(
            _fontDataHandle.AddrOfPinnedObject(),
            fontData.Length,
            fontSize,
            cfg,
            rangesHandle.AddrOfPinnedObject());

        rangesHandle.Free();
        MarseyLogger.Info($"[ImGuiRenderer] Font: {resourceName} @ {fontSize}px (scale={scale:F2})");
    }

    private static ushort[] BuildGlyphRanges() =>
    [
        0x0020, 0x00FF,
        0x0400, 0x052F,
        0xE000, 0xF8FF,
        0,
    ];

    private static nint _fontTextureId;
    private static uint _vao, _vbo, _ebo, _shader;
    private static uint _attribPos, _attribUv, _attribColor;
    private static int  _uniformTex, _uniformProjMtx;

    private const string VertSrc = @"#version 130
    uniform mat4 ProjMtx;
    in vec2 Position;
    in vec2 UV;
    in vec4 Color;
    out vec2 Frag_UV;
    out vec4 Frag_Color;
    void main() {
        Frag_UV = UV;
        Frag_Color = Color;
        gl_Position = ProjMtx * vec4(Position.xy, 0, 1);
    }";

    private const string FragSrc = @"#version 130
    uniform sampler2D Texture;
    in vec2 Frag_UV;
    in vec4 Frag_Color;
    out vec4 Out_Color;
    void main() {
        Out_Color = Frag_Color * texture(Texture, Frag_UV.st);
    }";

    private static void CreateGlObjects()
    {
        _shader         = GlBackend.CreateShader(VertSrc, FragSrc);
        _uniformTex     = GlBackend.GetUniformLocation(_shader, "Texture");
        _uniformProjMtx = GlBackend.GetUniformLocation(_shader, "ProjMtx");
        _attribPos      = (uint)GlBackend.GetAttribLocation(_shader, "Position");
        _attribUv       = (uint)GlBackend.GetAttribLocation(_shader, "UV");
        _attribColor    = (uint)GlBackend.GetAttribLocation(_shader, "Color");
        _vao            = GlBackend.GenVertexArray();
        _vbo            = GlBackend.GenBuffer();
        _ebo            = GlBackend.GenBuffer();

        if (_fontDataHandle.IsAllocated)
            _fontDataHandle.Free();
    }

    private static nint CreateFontTexture(nint pixels, int width, int height)
    {
        var tex = GlBackend.GenTexture();
        GlBackend.BindTexture(0x0DE1, tex);
        GlBackend.TexParameteri(0x0DE1, 0x2801, 0x2600);
        GlBackend.TexParameteri(0x0DE1, 0x2800, 0x2600);
        GlBackend.TexImage2D(width, height, pixels);
        GlBackend.BindTexture(0x0DE1, 0);
        return (nint)tex;
    }

    private static unsafe void RenderDrawData(ImDrawDataPtr drawData)
    {
        if (drawData.CmdListsCount == 0) return;

        var fbWidth  = (int)(drawData.DisplaySize.X * drawData.FramebufferScale.X);
        var fbHeight = (int)(drawData.DisplaySize.Y * drawData.FramebufferScale.Y);
        if (fbWidth <= 0 || fbHeight <= 0) return;

        GlBackend.UseProgram(_shader);
        GlBackend.Uniform1i(_uniformTex, 0);

        float L = drawData.DisplayPos.X;
        float R = drawData.DisplayPos.X + drawData.DisplaySize.X;
        float T = drawData.DisplayPos.Y;
        float B = drawData.DisplayPos.Y + drawData.DisplaySize.Y;
        float[] proj =
        [
             2f/(R-L),     0,          0, 0,
             0,            2f/(T-B),   0, 0,
             0,            0,         -1, 0,
            (R+L)/(L-R), (T+B)/(B-T), 0, 1,
        ];
        GlBackend.UniformMatrix4fv(_uniformProjMtx, proj);

        GlBackend.BindVertexArray(_vao);
        GlBackend.BindBuffer(0x8892, _vbo);
        GlBackend.BindBuffer(0x8893, _ebo);
        GlBackend.EnableVertexAttribArray(_attribPos);
        GlBackend.EnableVertexAttribArray(_attribUv);
        GlBackend.EnableVertexAttribArray(_attribColor);

        var stride = sizeof(ImDrawVert);
        GlBackend.VertexAttribPointer(_attribPos,   2, 0x1406, false, stride, 0);
        GlBackend.VertexAttribPointer(_attribUv,    2, 0x1406, false, stride, 8);
        GlBackend.VertexAttribPointer(_attribColor, 4, 0x1401, true,  stride, 16);

        var clipOff   = drawData.DisplayPos;
        var clipScale = drawData.FramebufferScale;

        for (int n = 0; n < drawData.CmdListsCount; n++)
        {
            var cmdList = drawData.CmdLists[n];
            GlBackend.BufferData(0x8892, cmdList.VtxBuffer.Size * stride, cmdList.VtxBuffer.Data, 0x88E0);
            GlBackend.BufferData(0x8893, cmdList.IdxBuffer.Size * sizeof(ushort), cmdList.IdxBuffer.Data, 0x88E0);

            for (int ci = 0; ci < cmdList.CmdBuffer.Size; ci++)
            {
                var cmd = cmdList.CmdBuffer[ci];
                if (cmd.UserCallback != nint.Zero) continue;

                var clipMin = new Vector2(
                    (cmd.ClipRect.X - clipOff.X) * clipScale.X,
                    (cmd.ClipRect.Y - clipOff.Y) * clipScale.Y);
                var clipMax = new Vector2(
                    (cmd.ClipRect.Z - clipOff.X) * clipScale.X,
                    (cmd.ClipRect.W - clipOff.Y) * clipScale.Y);

                if (clipMax.X <= clipMin.X || clipMax.Y <= clipMin.Y) continue;

                GlBackend.Scissor(
                    (int)clipMin.X,
                    (int)(fbHeight - clipMax.Y),
                    (int)(clipMax.X - clipMin.X),
                    (int)(clipMax.Y - clipMin.Y));

                GlBackend.ActiveTexture(0x84C0);
                GlBackend.BindTexture(0x0DE1, (uint)cmd.TextureId);
                GlBackend.DrawElementsBaseVertex(
                    0x0004, (int)cmd.ElemCount, 0x1403,
                    (nint)(cmd.IdxOffset * sizeof(ushort)),
                    (int)cmd.VtxOffset);
            }
        }
    }

    private static void DrawUI()
    {
        if (ShowDemoWindow)
            ImGui.ShowDemoWindow(ref ShowDemoWindow);

        ImGui.SetNextWindowPos(new Vector2(10, 10), ImGuiCond.Always);
        ImGui.SetNextWindowBgAlpha(0.85f);

        if (ImGui.Begin("## status",
                ImGuiWindowFlags.NoDecoration |
                ImGuiWindowFlags.NoInputs     |
                ImGuiWindowFlags.NoNav        |
                ImGuiWindowFlags.NoMove       |
                ImGuiWindowFlags.AlwaysAutoResize))
        {
            ImGui.TextColored(new Vector4(0.4f, 1f, 0.4f, 1f), "Render Hook: ACTIVE");
            ImGui.Text($"Screen: {_screenW}x{_screenH}");
        }
        ImGui.End();

        ImGui.SetNextWindowPos(new Vector2(10, 80), ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowBgAlpha(0.85f);

        if (ImGui.Begin("Despada Dev"))
        {
            if (ImGui.Button("Test Toast (3s)"))
                ShowToast("Тест", "Уведомление работает! 🎉", 3f);

            ImGui.SameLine();
            if (ImGui.Button("Test Long (8s)"))
                ShowToast("Долгое уведомление", "Это уведомление живёт 8 секунд.", 8f);

            ImGui.SameLine();
            if (ImGui.Button("Spam (x5)"))
            {
                for (int i = 1; i <= 5; i++)
                    ShowToast($"Уведомление #{i}", $"Тест стека уведомлений #{i}", 4f);
            }
        }
        ImGui.End();
    }

    public static void Shutdown()
    {
        if (!_initialized) return;
        if (_fontDataHandle.IsAllocated) _fontDataHandle.Free();
        ImGui.DestroyContext();
        _initialized = false;
        MarseyLogger.Info("[ImGuiRenderer] Shutdown complete.");
        NativeLoader.Cleanup();
    }

    private static void SetCursorVisible(bool visible)
    {
        try
        {
            var clydeType = AccessTools.TypeByName("Robust.Client.Graphics.Clyde.Clyde");
            if (clydeType is null) return;

            var clyde = ResolveFromIoC("Robust.Client.Graphics.IClyde");
            if (clyde is null) return;

            var setCursor = AccessTools.Method(clydeType, "SetCursor");
            if (setCursor is null) return;

            if (visible)
            {
                setCursor.Invoke(clyde, [null]);
            }
            else
            {
                var shapeType = AccessTools.TypeByName("Robust.Client.Graphics.StandardCursorShape");
                if (shapeType is not null && Enum.IsDefined(shapeType, "Arrow"))
                {
                    var shape  = Enum.Parse(shapeType, "Arrow");
                    var getStd = AccessTools.Method(clydeType, "GetStandardCursor");
                    var cursor = getStd?.Invoke(clyde, [shape]);
                    setCursor.Invoke(clyde, [cursor]);
                }
            }

            MarseyLogger.Info($"[ImGuiRenderer] Cursor visible={visible}");
        }
        catch (Exception ex)
        {
            MarseyLogger.Warn($"[ImGuiRenderer] SetCursorVisible failed: {ex.Message}");
        }
    }

    private static object? ResolveFromIoC(string interfaceName)
    {
        try
        {
            var iocType       = AccessTools.TypeByName("Robust.Shared.IoC.IoCManager");
            if (iocType is null) return null;
            var interfaceType = AccessTools.TypeByName(interfaceName);
            if (interfaceType is null) return null;

            var resolveGeneric = iocType.GetMethod("Resolve",
                BindingFlags.Static | BindingFlags.Public, null, [], null);
            if (resolveGeneric is not null)
                return resolveGeneric.MakeGenericMethod(interfaceType).Invoke(null, []);

            var resolveByType = iocType.GetMethod("Resolve",
                BindingFlags.Static | BindingFlags.Public, null, [typeof(Type)], null);
            return resolveByType?.Invoke(null, [interfaceType]);
        }
        catch (Exception ex)
        {
            MarseyLogger.Warn($"[ImGuiRenderer] ResolveFromIoC({interfaceName}) failed: {ex.Message}");
            return null;
        }
    }
}