using System.Runtime.InteropServices;

namespace Despada.ImGui.Hook;
internal static class GlStateGuard
{
    private const string LibGL_Win   = "opengl32.dll";
    private const string LibGL_Linux = "libGL.so.1";
    private const string LibGL_Mac   = "/System/Library/Frameworks/OpenGL.framework/OpenGL";

    [DllImport(LibGL_Win,   EntryPoint = "glGetIntegerv")] private static extern void GetIntegerv_Win  (uint pname, out int data);
    [DllImport(LibGL_Linux, EntryPoint = "glGetIntegerv")] private static extern void GetIntegerv_Linux(uint pname, out int data);
    [DllImport(LibGL_Mac,   EntryPoint = "glGetIntegerv")] private static extern void GetIntegerv_Mac  (uint pname, out int data);

    [DllImport(LibGL_Win,   EntryPoint = "glBindVertexArray")] private static extern void BindVAO_Win  (uint array);
    [DllImport(LibGL_Linux, EntryPoint = "glBindVertexArray")] private static extern void BindVAO_Linux(uint array);
    [DllImport(LibGL_Mac,   EntryPoint = "glBindVertexArray")] private static extern void BindVAO_Mac  (uint array);

    [DllImport(LibGL_Win,   EntryPoint = "glUseProgram")] private static extern void UseProgram_Win  (uint program);
    [DllImport(LibGL_Linux, EntryPoint = "glUseProgram")] private static extern void UseProgram_Linux(uint program);
    [DllImport(LibGL_Mac,   EntryPoint = "glUseProgram")] private static extern void UseProgram_Mac  (uint program);

    [DllImport(LibGL_Win,   EntryPoint = "glEnable")]  private static extern void Enable_Win  (uint cap);
    [DllImport(LibGL_Linux, EntryPoint = "glEnable")]  private static extern void Enable_Linux(uint cap);
    [DllImport(LibGL_Mac,   EntryPoint = "glEnable")]  private static extern void Enable_Mac  (uint cap);

    [DllImport(LibGL_Win,   EntryPoint = "glDisable")] private static extern void Disable_Win  (uint cap);
    [DllImport(LibGL_Linux, EntryPoint = "glDisable")] private static extern void Disable_Linux(uint cap);
    [DllImport(LibGL_Mac,   EntryPoint = "glDisable")] private static extern void Disable_Mac  (uint cap);

    [DllImport(LibGL_Win,   EntryPoint = "glIsEnabled")] [return: MarshalAs(UnmanagedType.U1)] private static extern bool IsEnabled_Win  (uint cap);
    [DllImport(LibGL_Linux, EntryPoint = "glIsEnabled")] [return: MarshalAs(UnmanagedType.U1)] private static extern bool IsEnabled_Linux(uint cap);
    [DllImport(LibGL_Mac,   EntryPoint = "glIsEnabled")] [return: MarshalAs(UnmanagedType.U1)] private static extern bool IsEnabled_Mac  (uint cap);

    [DllImport(LibGL_Win,   EntryPoint = "glBlendFuncSeparate")] private static extern void BlendFuncSep_Win  (uint sfRGB, uint dfRGB, uint sfA, uint dfA);
    [DllImport(LibGL_Linux, EntryPoint = "glBlendFuncSeparate")] private static extern void BlendFuncSep_Linux(uint sfRGB, uint dfRGB, uint sfA, uint dfA);
    [DllImport(LibGL_Mac,   EntryPoint = "glBlendFuncSeparate")] private static extern void BlendFuncSep_Mac  (uint sfRGB, uint dfRGB, uint sfA, uint dfA);

    [DllImport(LibGL_Win,   EntryPoint = "glBlendEquationSeparate")] private static extern void BlendEqSep_Win  (uint modeRGB, uint modeA);
    [DllImport(LibGL_Linux, EntryPoint = "glBlendEquationSeparate")] private static extern void BlendEqSep_Linux(uint modeRGB, uint modeA);
    [DllImport(LibGL_Mac,   EntryPoint = "glBlendEquationSeparate")] private static extern void BlendEqSep_Mac  (uint modeRGB, uint modeA);

    [DllImport(LibGL_Win,   EntryPoint = "glActiveTexture")] private static extern void ActiveTexture_Win  (uint texture);
    [DllImport(LibGL_Linux, EntryPoint = "glActiveTexture")] private static extern void ActiveTexture_Linux(uint texture);
    [DllImport(LibGL_Mac,   EntryPoint = "glActiveTexture")] private static extern void ActiveTexture_Mac  (uint texture);

    [DllImport(LibGL_Win,   EntryPoint = "glPolygonMode")] private static extern void PolygonMode_Win  (uint face, uint mode);
    [DllImport(LibGL_Linux, EntryPoint = "glPolygonMode")] private static extern void PolygonMode_Linux(uint face, uint mode);
    [DllImport(LibGL_Mac,   EntryPoint = "glPolygonMode")] private static extern void PolygonMode_Mac  (uint face, uint mode);

    private const uint GL_BLEND                     = 0x0BE2;
    private const uint GL_SCISSOR_TEST              = 0x0C11;
    private const uint GL_DEPTH_TEST                = 0x0B71;
    private const uint GL_CULL_FACE                 = 0x0B44;
    private const uint GL_STENCIL_TEST              = 0x0B90;
    private const uint GL_FRAMEBUFFER_SRGB          = 0x8DB9;
    private const uint GL_ARRAY_BUFFER_BINDING      = 0x8894;
    private const uint GL_ELEMENT_ARRAY_BUFFER_BINDING = 0x8895;
    private const uint GL_VERTEX_ARRAY_BINDING      = 0x85B5;
    private const uint GL_CURRENT_PROGRAM           = 0x8B8D;
    private const uint GL_BLEND_SRC_RGB             = 0x80C9;
    private const uint GL_BLEND_DST_RGB             = 0x80CA;
    private const uint GL_BLEND_SRC_ALPHA           = 0x80CB;
    private const uint GL_BLEND_DST_ALPHA           = 0x80CC;
    private const uint GL_BLEND_EQUATION_RGB        = 0x8009;
    private const uint GL_BLEND_EQUATION_ALPHA      = 0x883D;
    private const uint GL_ACTIVE_TEXTURE            = 0x84E0;
    private const uint GL_TEXTURE_BINDING_2D        = 0x8069;
    private const uint GL_TEXTURE0                  = 0x84C0;
    private const uint GL_POLYGON_MODE              = 0x0B40;
    private const uint GL_FRONT_AND_BACK            = 0x0408;
    private const uint GL_VIEWPORT                  = 0x0BA2;

    [DllImport(LibGL_Win,   EntryPoint = "glGetIntegerv")] private static extern void GetIntegervArr_Win  (uint pname, int[] data);
    [DllImport(LibGL_Linux, EntryPoint = "glGetIntegerv")] private static extern void GetIntegervArr_Linux(uint pname, int[] data);
    [DllImport(LibGL_Mac,   EntryPoint = "glGetIntegerv")] private static extern void GetIntegervArr_Mac  (uint pname, int[] data);

    private static readonly bool _isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
    private static readonly bool _isMacOS   = RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
    public static (int width, int height) GetFramebufferSize()
    {
        var vp = new int[4];
        if (_isWindows)    GetIntegervArr_Win  (GL_VIEWPORT, vp);
        else if (_isMacOS) GetIntegervArr_Mac  (GL_VIEWPORT, vp);
        else               GetIntegervArr_Linux(GL_VIEWPORT, vp);
        return (vp[2], vp[3]);
    }

    private static void GetInt(uint pname, out int data)
    {
        if (_isWindows)    GetIntegerv_Win  (pname, out data);
        else if (_isMacOS) GetIntegerv_Mac  (pname, out data);
        else               GetIntegerv_Linux(pname, out data);
    }
    private static void BindVAO(uint v)     { if (_isWindows) BindVAO_Win(v); else if (_isMacOS) BindVAO_Mac(v); else BindVAO_Linux(v); }
    private static void UseProgram(uint p)  { if (_isWindows) UseProgram_Win(p); else if (_isMacOS) UseProgram_Mac(p); else UseProgram_Linux(p); }
    private static void EnableCap(uint c)   { if (_isWindows) Enable_Win(c); else if (_isMacOS) Enable_Mac(c); else Enable_Linux(c); }
    private static void DisableCap(uint c)  { if (_isWindows) Disable_Win(c); else if (_isMacOS) Disable_Mac(c); else Disable_Linux(c); }
    private static bool IsCapEnabled(uint c){ if (_isWindows) return IsEnabled_Win(c); else if (_isMacOS) return IsEnabled_Mac(c); else return IsEnabled_Linux(c); }
    private static void BlendFuncSeparate(uint a, uint b, uint c, uint d) { if (_isWindows) BlendFuncSep_Win(a,b,c,d); else if (_isMacOS) BlendFuncSep_Mac(a,b,c,d); else BlendFuncSep_Linux(a,b,c,d); }
    private static void BlendEquationSeparate(uint a, uint b) { if (_isWindows) BlendEqSep_Win(a,b); else if (_isMacOS) BlendEqSep_Mac(a,b); else BlendEqSep_Linux(a,b); }
    private static void ActiveTexture(uint t) { if (_isWindows) ActiveTexture_Win(t); else if (_isMacOS) ActiveTexture_Mac(t); else ActiveTexture_Linux(t); }
    private static void SetPolygonMode(uint f, uint m) { if (_isWindows) PolygonMode_Win(f,m); else if (_isMacOS) PolygonMode_Mac(f,m); else PolygonMode_Linux(f,m); }

    private struct GlSnapshot
    {
        public int  LastProgram, LastVAO, LastArrayBuffer, LastElementBuffer;
        public int  LastActiveTexture, LastTexture;
        public int  LastBlendSrcRGB, LastBlendDstRGB, LastBlendSrcAlpha, LastBlendDstAlpha;
        public int  LastBlendEqRGB, LastBlendEqAlpha, LastPolygonMode;
        public bool BlendEnabled, ScissorEnabled, DepthEnabled, CullEnabled, StencilEnabled;
        public bool SrgbEnabled; 
    }

    [ThreadStatic] private static GlSnapshot _snapshot;

    public static void Save()
    {
        GetInt(GL_CURRENT_PROGRAM,              out _snapshot.LastProgram);
        GetInt(GL_VERTEX_ARRAY_BINDING,         out _snapshot.LastVAO);
        GetInt(GL_ARRAY_BUFFER_BINDING,         out _snapshot.LastArrayBuffer);
        GetInt(GL_ELEMENT_ARRAY_BUFFER_BINDING, out _snapshot.LastElementBuffer);
        GetInt(GL_ACTIVE_TEXTURE,               out _snapshot.LastActiveTexture);
        ActiveTexture(GL_TEXTURE0);
        GetInt(GL_TEXTURE_BINDING_2D,           out _snapshot.LastTexture);
        GetInt(GL_BLEND_SRC_RGB,                out _snapshot.LastBlendSrcRGB);
        GetInt(GL_BLEND_DST_RGB,                out _snapshot.LastBlendDstRGB);
        GetInt(GL_BLEND_SRC_ALPHA,              out _snapshot.LastBlendSrcAlpha);
        GetInt(GL_BLEND_DST_ALPHA,              out _snapshot.LastBlendDstAlpha);
        GetInt(GL_BLEND_EQUATION_RGB,           out _snapshot.LastBlendEqRGB);
        GetInt(GL_BLEND_EQUATION_ALPHA,         out _snapshot.LastBlendEqAlpha);
        GetInt(GL_POLYGON_MODE,                 out _snapshot.LastPolygonMode);

        _snapshot.BlendEnabled   = IsCapEnabled(GL_BLEND);
        _snapshot.ScissorEnabled = IsCapEnabled(GL_SCISSOR_TEST);
        _snapshot.DepthEnabled   = IsCapEnabled(GL_DEPTH_TEST);
        _snapshot.CullEnabled    = IsCapEnabled(GL_CULL_FACE);
        _snapshot.StencilEnabled = IsCapEnabled(GL_STENCIL_TEST);
        _snapshot.SrgbEnabled    = IsCapEnabled(GL_FRAMEBUFFER_SRGB);
        if (_snapshot.SrgbEnabled)
            DisableCap(GL_FRAMEBUFFER_SRGB);
    }

    public static void Restore()
    {
        UseProgram((uint)_snapshot.LastProgram);
        BindVAO((uint)_snapshot.LastVAO);
        BlendFuncSeparate(
            (uint)_snapshot.LastBlendSrcRGB, (uint)_snapshot.LastBlendDstRGB,
            (uint)_snapshot.LastBlendSrcAlpha, (uint)_snapshot.LastBlendDstAlpha);
        BlendEquationSeparate((uint)_snapshot.LastBlendEqRGB, (uint)_snapshot.LastBlendEqAlpha);
        SetPolygonMode(GL_FRONT_AND_BACK, (uint)_snapshot.LastPolygonMode);

        RestoreCap(GL_BLEND,            _snapshot.BlendEnabled);
        RestoreCap(GL_SCISSOR_TEST,     _snapshot.ScissorEnabled);
        RestoreCap(GL_DEPTH_TEST,       _snapshot.DepthEnabled);
        RestoreCap(GL_CULL_FACE,        _snapshot.CullEnabled);
        RestoreCap(GL_STENCIL_TEST,     _snapshot.StencilEnabled);
        RestoreCap(GL_FRAMEBUFFER_SRGB, _snapshot.SrgbEnabled);

        ActiveTexture((uint)_snapshot.LastActiveTexture);
    }

    private static void RestoreCap(uint cap, bool wasEnabled)
    {
        if (wasEnabled) EnableCap(cap); else DisableCap(cap);
    }
}