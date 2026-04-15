using System.Numerics;
using Despada.ImGui;
using ImGuiNET;

namespace Despada.ImGui.UserInterface;

public static class Widgets
{
    private static int _sectionId;
    private static readonly HashSet<string> _toggleUpdatedThisFrame = new();

    public static void ResetFrame()
    {
        _sectionId = 0;
        _toggleUpdatedThisFrame.Clear();
    }

    private static readonly Vector4 SectionBg     = Theme.Hex(0x141418);
    private static readonly uint    SectionBorder  = Theme.ToU32(Theme.HexA(0xFFFFFF, 0.08f));
    private static readonly uint    SeparatorColor = Theme.ToU32(Theme.HexA(0xFFFFFF, 0.08f));

    private const float SectionPad   = 12f;
    private const float SectionGap   = 14f;
    private const float SectionRound = 10f;
    private const float ContentIndent = 16f;
    private const float HeaderPad    = 16f;

    public static void BeginSection(string title)
    {
        _sectionId++;
        var availW = ImGuiNET.ImGui.GetContentRegionAvail().X;

        PushSectionStyle();

        ImGuiNET.ImGui.BeginChild($"##section_{_sectionId}", new Vector2(availW, 0f),
            ImGuiChildFlags.AutoResizeY | ImGuiChildFlags.AlwaysAutoResize,
            ImGuiWindowFlags.NoScrollbar);

        DrawSectionBorder();

        ImGuiNET.ImGui.TextColored(Theme.TextSecondary, title);
        ImGuiNET.ImGui.Spacing();
    }

    public static void EndSection()
    {
        ImGuiNET.ImGui.EndChild();
        PopSectionStyle();
        ImGuiNET.ImGui.Dummy(new Vector2(0f, SectionGap));
    }

    private const float HeaderH = 64f;

    public static bool BeginFeatureSection(string icon, string title, string description, ref bool enabled)
    {
        _sectionId++;
        var availW = ImGuiNET.ImGui.GetContentRegionAvail().X;

        PushSectionStyle();

        ImGuiNET.ImGui.BeginChild($"##feature_{_sectionId}", new Vector2(availW, 0f),
            ImGuiChildFlags.AutoResizeY | ImGuiChildFlags.AlwaysAutoResize,
            ImGuiWindowFlags.NoScrollbar);

        DrawSectionBorder();

        var dl       = ImGuiNET.ImGui.GetWindowDrawList();
        var font     = ImGuiNET.ImGui.GetFont();
        var contentW = ImGuiNET.ImGui.GetContentRegionAvail().X;
        var origin   = ImGuiNET.ImGui.GetCursorScreenPos();

        ImGuiNET.ImGui.Dummy(new Vector2(contentW, HeaderH));

        var hL = origin.X + HeaderPad;
        var hR = origin.X + contentW - HeaderPad;

        var toggleId = $"##ft_{_sectionId}";
        var animT = GetToggleT(toggleId, enabled);

        var iconFs = font.FontSize * 2.0f;
        var iconSz = font.CalcTextSizeA(iconFs, float.MaxValue, 0f, icon);
        var iconCol = new Vector4(
            Theme.TextSecondary.X + (Theme.Cyan.X - Theme.TextSecondary.X) * animT,
            Theme.TextSecondary.Y + (Theme.Cyan.Y - Theme.TextSecondary.Y) * animT,
            Theme.TextSecondary.Z + (Theme.Cyan.Z - Theme.TextSecondary.Z) * animT,
            1f);
        dl.AddText(font, iconFs,
            new Vector2(hL, origin.Y + (HeaderH - iconSz.Y) * 0.5f),
            Theme.ToU32(iconCol), icon);

        var textX   = hL + iconSz.X + 14f;
        var titleFs = font.FontSize * 1.1f;
        var titleSz = font.CalcTextSizeA(titleFs, float.MaxValue, 0f, title);
        var descSz  = ImGuiNET.ImGui.CalcTextSize(description);
        var blockH  = titleSz.Y + descSz.Y + 3f;
        var blockY  = origin.Y + (HeaderH - blockH) * 0.5f;

        if (enabled)
            DrawGradientText(dl, font, titleFs, new Vector2(textX, blockY),
                title, Theme.Violet, Theme.Cyan, Theme.Magenta);
        else
            dl.AddText(font, titleFs, new Vector2(textX, blockY),
                Theme.ToU32(Theme.TextSecondary), title);

        dl.AddText(new Vector2(textX, blockY + titleSz.Y + 3f),
            Theme.ToU32(Theme.TextDisabled), description);

        var toggleX = hR - ToggleW;
        var toggleY = origin.Y + (HeaderH - ToggleH) * 0.5f;
        var tMin = new Vector2(toggleX, toggleY);
        var tMax = new Vector2(toggleX + ToggleW, toggleY + ToggleH);

        bool tHov = ImGuiNET.ImGui.IsMouseHoveringRect(tMin, tMax);
        if (tHov && ImGuiNET.ImGui.IsMouseClicked(ImGuiMouseButton.Left))
            enabled = !enabled;

        DrawToggleAt(dl, tMin, enabled, tHov, toggleId);

        if (enabled)
        {
            var sepY = ImGuiNET.ImGui.GetCursorScreenPos().Y;
            dl.AddRectFilled(
                new Vector2(origin.X, sepY),
                new Vector2(origin.X + contentW, sepY + 1f),
                SeparatorColor);
            ImGuiNET.ImGui.Dummy(new Vector2(0f, 8f));
        }

        ImGuiNET.ImGui.Indent(ContentIndent);
        return enabled;
    }

    public static void EndFeatureSection()
    {
        ImGuiNET.ImGui.Unindent(ContentIndent);
        ImGuiNET.ImGui.EndChild();
        PopSectionStyle();
        ImGuiNET.ImGui.Dummy(new Vector2(0f, SectionGap));
    }

    private static void PushSectionStyle()
    {
        ImGuiNET.ImGui.PushStyleColor(ImGuiCol.ChildBg, SectionBg);
        ImGuiNET.ImGui.PushStyleVar(ImGuiStyleVar.ChildRounding, SectionRound);
        ImGuiNET.ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(SectionPad, SectionPad));
    }

    private static void PopSectionStyle()
    {
        ImGuiNET.ImGui.PopStyleVar(2);
        ImGuiNET.ImGui.PopStyleColor();
    }

    private static void DrawSectionBorder()
    {
        var dl  = ImGuiNET.ImGui.GetWindowDrawList();
        var pos = ImGuiNET.ImGui.GetWindowPos();
        var sz  = ImGuiNET.ImGui.GetWindowSize();
        dl.AddRect(pos, new Vector2(pos.X + sz.X, pos.Y + sz.Y),
            SectionBorder, SectionRound, ImDrawFlags.None, 1f);
    }

    private const float ToggleW     = 46f;
    private const float ToggleH     = 24f;
    private const float KnobR       = 9f;
    private const float KnobPad     = 3f;
    private const float ToggleSpeed = 20f;

    private static readonly Dictionary<string, float> _toggleAnim = new();

    private static float GetToggleT(string id, bool value)
    {
        if (!_toggleAnim.TryGetValue(id, out var t))
            t = value ? 1f : 0f;

        if (_toggleUpdatedThisFrame.Add(id))
        {
            var target = value ? 1f : 0f;
            t = Anim.Lerp(t, target, ToggleSpeed, ImGuiNET.ImGui.GetIO().DeltaTime);
            _toggleAnim[id] = t;
        }

        return t;
    }

    private static void DrawToggleAt(ImDrawListPtr dl, Vector2 pos, bool value, bool hovered, string id)
    {
        var t = GetToggleT(id, value);

        var min    = pos;
        var max    = new Vector2(pos.X + ToggleW, pos.Y + ToggleH);
        var radius = ToggleH * 0.5f;

        var offColor = hovered ? Theme.BgHover : Theme.BgElevated;
        var onColor  = hovered ? Theme.Cyan : Theme.CyanDim;
        var tc = new Vector4(
            offColor.X + (onColor.X - offColor.X) * t,
            offColor.Y + (onColor.Y - offColor.Y) * t,
            offColor.Z + (onColor.Z - offColor.Z) * t,
            offColor.W + (onColor.W - offColor.W) * t);
        dl.AddRectFilled(min, max, Theme.ToU32(tc), radius);

        if (t > 0.01f)
        {
            var gradAlpha = t * 0.25f;
            var gradL = Theme.ToU32(Theme.HexA(0x6B2FD6, gradAlpha));
            var gradR = Theme.ToU32(Theme.HexA(0xFF3090, gradAlpha));
            var inset = radius;
            dl.AddRectFilledMultiColor(
                new Vector2(min.X + inset, min.Y),
                new Vector2(max.X - inset, max.Y),
                gradL, gradR, gradR, gradL);
        }

        var knobOff = min.X + KnobR + KnobPad;
        var knobOn  = max.X - KnobR - KnobPad;
        var knobX   = knobOff + (knobOn - knobOff) * t;

        var knobAlpha = 0.4f + 0.6f * t;
        var knobColor = Theme.HexA(0xFFFFFF, knobAlpha);
        dl.AddCircleFilled(new Vector2(knobX, min.Y + ToggleH * 0.5f), KnobR, Theme.ToU32(knobColor));
    }

    public static bool Toggle(string id, ref bool value)
    {
        var pos = ImGuiNET.ImGui.GetCursorScreenPos();

        ImGuiNET.ImGui.InvisibleButton(id, new Vector2(ToggleW, ToggleH));
        bool hovered = ImGuiNET.ImGui.IsItemHovered();

        bool clicked = hovered && ImGuiNET.ImGui.IsMouseClicked(ImGuiMouseButton.Left);
        if (clicked)
            value = !value;

        DrawToggleAt(ImGuiNET.ImGui.GetWindowDrawList(), pos, value, hovered, id);

        return clicked;
    }

    public static bool ToggleRow(string label, ref bool value)
    {
        ImGuiNET.ImGui.Spacing();

        var contentW = ImGuiNET.ImGui.GetContentRegionAvail().X;

        ImGuiNET.ImGui.AlignTextToFramePadding();
        ImGuiNET.ImGui.TextUnformatted(label);
        ImGuiNET.ImGui.SameLine(contentW - ToggleW);
        var result = Toggle($"##{label}_tgl", ref value);

        ImGuiNET.ImGui.Spacing();
        return result;
    }

    private const float CheckboxSize  = 28f;
    private const float CheckboxRound = 6f;

    public static bool Checkbox(string id, ref bool value)
    {
        var pos = ImGuiNET.ImGui.GetCursorScreenPos();

        ImGuiNET.ImGui.InvisibleButton(id, new Vector2(CheckboxSize, CheckboxSize));
        bool hovered = ImGuiNET.ImGui.IsItemHovered();

        bool clicked = hovered && ImGuiNET.ImGui.IsMouseClicked(ImGuiMouseButton.Left);
        if (clicked)
        {
            value = !value;
            if (value)
            {
                var center = new Vector2(pos.X + CheckboxSize * 0.5f, pos.Y + CheckboxSize * 0.5f);
                SpawnBurstSparks(center, 12);
            }
        }

        var dl = ImGuiNET.ImGui.GetWindowDrawList();
        var t = GetToggleT(id, value);

        var min = pos;
        var max = new Vector2(pos.X + CheckboxSize, pos.Y + CheckboxSize);

        var bgOff = hovered ? Theme.BgHover : Theme.BgElevated;
        var bgOn  = Theme.Cyan;
        var bg = new Vector4(
            bgOff.X + (bgOn.X - bgOff.X) * t,
            bgOff.Y + (bgOn.Y - bgOff.Y) * t,
            bgOff.Z + (bgOn.Z - bgOff.Z) * t,
            bgOff.W + (bgOn.W - bgOff.W) * t);
        dl.AddRectFilled(min, max, Theme.ToU32(bg), CheckboxRound);

        if (t < 0.99f)
            dl.AddRect(min, max, SectionBorder, CheckboxRound, ImDrawFlags.None, 1f);

        if (t > 0.01f)
        {
            var cx = pos.X + CheckboxSize * 0.5f;
            var cy = pos.Y + CheckboxSize * 0.5f;
            var checkCol = Theme.ToU32(Theme.HexA(0xFFFFFF, t));

            Vector2[] checkPts =
            [
                new(cx - 5.5f, cy + 0.5f),
                new(cx - 1f,   cy + 5f),
                new(cx + 6.5f, cy - 4f)
            ];
            dl.AddPolyline(ref checkPts[0], 3, checkCol, ImDrawFlags.None, 3f);
        }

        return clicked;
    }

    public static bool CheckboxRow(string label, ref bool value)
    {
        ImGuiNET.ImGui.Spacing();

        var contentW = ImGuiNET.ImGui.GetContentRegionAvail().X;

        ImGuiNET.ImGui.AlignTextToFramePadding();
        ImGuiNET.ImGui.TextUnformatted(label);
        ImGuiNET.ImGui.SameLine(contentW - CheckboxSize);
        var result = Checkbox($"##{label}_cb", ref value);

        ImGuiNET.ImGui.Spacing();
        return result;
    }

    private const float SliderTrackH  = 6f;
    private const float SliderKnobR   = 7f;
    private const float SliderBoxW    = 72f;
    private const float SliderBoxPad  = 16f;

    private static readonly uint TrackBorder = Theme.ToU32(Theme.Border);
    private static readonly uint TrackFillL  = Theme.ToU32(Theme.Violet);
    private static readonly uint TrackFillR  = Theme.ToU32(Theme.Cyan);

    private struct Spark
    {
        public Vector2 Pos, Vel;
        public float Life, MaxLife, Size;
    }

    private static readonly List<Spark> _sparks = new();
    private static readonly Random _rng = new();

    private static void UpdateSparks(ImDrawListPtr dl, float dt)
    {
        for (int i = _sparks.Count - 1; i >= 0; i--)
        {
            var s = _sparks[i];
            s.Life -= dt;
            if (s.Life <= 0f) { _sparks.RemoveAt(i); continue; }

            s.Pos += s.Vel * dt;
            s.Vel.Y += 150f * dt;
            _sparks[i] = s;

            var alpha = s.Life / s.MaxLife;
            var t = 1f - alpha;
            var color = new Vector4(
                Theme.Violet.X + (Theme.Magenta.X - Theme.Violet.X) * t,
                Theme.Violet.Y + (Theme.Magenta.Y - Theme.Violet.Y) * t,
                Theme.Violet.Z + (Theme.Magenta.Z - Theme.Violet.Z) * t,
                alpha);

            dl.AddCircleFilled(s.Pos, s.Size * (0.5f + 0.5f * alpha), Theme.ToU32(color));
        }
    }

    private static void SpawnSparks(Vector2 origin, float dragVelX, int count)
    {
        for (int i = 0; i < count; i++)
        {
            var velX = -dragVelX * (0.05f + _rng.NextSingle() * 0.15f)
                     + (_rng.NextSingle() - 0.5f) * 50f;
            var velY = (_rng.NextSingle() - 0.6f) * 80f;

            var speed = MathF.Sqrt(velX * velX + velY * velY);
            if (speed > 200f) { velX *= 200f / speed; velY *= 200f / speed; }
            if (speed < 30f)  { velX *= 30f / speed;  velY *= 30f / speed; }

            var life = 0.6f + _rng.NextSingle() * 0.5f;

            _sparks.Add(new Spark
            {
                Pos     = origin + new Vector2((_rng.NextSingle() - 0.5f) * 6f, (_rng.NextSingle() - 0.5f) * 6f),
                Vel     = new Vector2(velX, velY),
                Life    = life,
                MaxLife = life,
                Size    = 1.5f + _rng.NextSingle() * 1.5f,
            });
        }
    }

    private static void SpawnBurstSparks(Vector2 center, int count)
    {
        for (int i = 0; i < count; i++)
        {
            var angle = _rng.NextSingle() * MathF.PI * 2f;
            var speed = 60f + _rng.NextSingle() * 120f;
            var velX = MathF.Cos(angle) * speed;
            var velY = MathF.Sin(angle) * speed - 20f;

            var life = 0.5f + _rng.NextSingle() * 0.5f;

            _sparks.Add(new Spark
            {
                Pos     = center + new Vector2((_rng.NextSingle() - 0.5f) * 6f, (_rng.NextSingle() - 0.5f) * 6f),
                Vel     = new Vector2(velX, velY),
                Life    = life,
                MaxLife = life,
                Size    = 1.5f + _rng.NextSingle() * 2f,
            });
        }
    }

    private static string? _editingSlider;
    private static string  _editBuffer = "";
    private static bool    _editFirstFrame;
    private static float   _lastSliderKnobX;

    private const float ArrowBtnW = 18f;

    public static bool SliderFloat(string label, ref float value, float min, float max,
        string format = "%.0f", string suffix = "", float step = 1f)
    {
        ImGuiNET.ImGui.Spacing();

        var dl       = ImGuiNET.ImGui.GetWindowDrawList();
        var contentW = ImGuiNET.ImGui.GetContentRegionAvail().X;
        var pos      = ImGuiNET.ImGui.GetCursorScreenPos();
        var dt       = ImGuiNET.ImGui.GetIO().DeltaTime;

        var displayLabel = label.Contains("##") ? label[..label.IndexOf("##")] : label;
        if (string.IsNullOrEmpty(displayLabel))
        {
            var colonIdx = format.IndexOf(':');
            displayLabel = colonIdx >= 0 ? format[..colonIdx].Trim() : "Value";
        }

        var decimals = 0;
        var dotIdx = format.IndexOf('.');
        if (dotIdx >= 0 && dotIdx + 1 < format.Length && char.IsDigit(format[dotIdx + 1]))
            decimals = format[dotIdx + 1] - '0';

        var valueStr   = value.ToString($"F{decimals}");
        var displayVal = string.IsNullOrEmpty(suffix) ? valueStr : $"{valueStr} {suffix}";

        var labelH = ImGuiNET.ImGui.GetTextLineHeightWithSpacing();
        var changed = false;

        ImGuiNET.ImGui.TextUnformatted(displayLabel.Trim());

        var showArrows = step > 0f;
        var totalBoxW  = SliderBoxW + (showArrows ? ArrowBtnW * 2f + 4f : 0f);
        var boxH       = labelH + 6f;
        var groupRight  = pos.X + contentW - SliderBoxPad;
        var groupLeft   = groupRight - totalBoxW;

        var isEditing = _editingSlider == label;

        if (showArrows)
        {
            var arrowLMin = new Vector2(groupLeft, pos.Y - 1f);
            var arrowLMax = new Vector2(arrowLMin.X + ArrowBtnW, arrowLMin.Y + boxH);

            bool arrowLHov = ImGuiNET.ImGui.IsMouseHoveringRect(arrowLMin, arrowLMax);
            dl.AddRectFilled(arrowLMin, arrowLMax,
                Theme.ToU32(arrowLHov ? Theme.BgHover : Theme.BgElevated), 4f, ImDrawFlags.RoundCornersLeft);

            var aSzL = ImGuiNET.ImGui.CalcTextSize("<");
            dl.AddText(
                new Vector2(arrowLMin.X + (ArrowBtnW - aSzL.X) * 0.5f, arrowLMin.Y + (boxH - aSzL.Y) * 0.5f),
                Theme.ToU32(arrowLHov ? Theme.TextPrimary : Theme.TextSecondary), "<");

            if (arrowLHov && ImGuiNET.ImGui.IsMouseClicked(ImGuiMouseButton.Left))
            {
                value = MathF.Max(min, value - step);
                changed = true;
            }
        }

        var boxX   = groupLeft + (showArrows ? ArrowBtnW + 2f : 0f);
        var boxMin = new Vector2(boxX, pos.Y - 1f);
        var boxMax = new Vector2(boxX + SliderBoxW, boxMin.Y + boxH);

        if (isEditing)
        {
            dl.AddRectFilled(boxMin, boxMax, Theme.ToU32(SectionBg), showArrows ? 0f : 4f);
            dl.AddRect(boxMin, boxMax, Theme.ToU32(Theme.BorderActive), showArrows ? 0f : 4f);

            var io = ImGuiNET.ImGui.GetIO();

            for (int ci = 0; ci < io.InputQueueCharacters.Size; ci++)
            {
                var ch = (char)io.InputQueueCharacters[ci];
                if (ch >= 32 && (char.IsDigit(ch) || ch == '.' || ch == '-' || ch == ',') && _editBuffer.Length < 4)
                    _editBuffer += ch;
            }

            if (ImGuiNET.ImGui.IsKeyPressed(ImGuiKey.Backspace) && _editBuffer.Length > 0)
                _editBuffer = _editBuffer[..^1];

            if (ImGuiNET.ImGui.IsKeyPressed(ImGuiKey.Enter) || ImGuiNET.ImGui.IsKeyPressed(ImGuiKey.KeypadEnter))
            {
                if (float.TryParse(_editBuffer, System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture, out var parsed))
                    value = MathF.Max(min, MathF.Min(max, parsed));
                _editingSlider = null;
                changed = true;
            }

            if (ImGuiNET.ImGui.IsKeyPressed(ImGuiKey.Escape))
                _editingSlider = null;

            if (ImGuiNET.ImGui.IsMouseClicked(ImGuiMouseButton.Left) &&
                !ImGuiNET.ImGui.IsMouseHoveringRect(boxMin, boxMax))
            {
                if (float.TryParse(_editBuffer, System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture, out var parsed))
                    value = MathF.Max(min, MathF.Min(max, parsed));
                _editingSlider = null;
                changed = true;
            }

            var bufSz = ImGuiNET.ImGui.CalcTextSize(_editBuffer);
            var bufPos = new Vector2(
                boxMin.X + (SliderBoxW - bufSz.X) * 0.5f,
                boxMin.Y + (boxH - bufSz.Y) * 0.5f);
            dl.AddText(bufPos, Theme.ToU32(Theme.TextPrimary), _editBuffer);

            var cursorOn = ((int)(ImGuiNET.ImGui.GetTime() * 2.5f)) % 2 == 0;
            if (cursorOn)
            {
                var cursorX = bufPos.X + bufSz.X + 1f;
                var cursorY1 = boxMin.Y + 4f;
                var cursorY2 = boxMax.Y - 4f;
                dl.AddLine(new Vector2(cursorX, cursorY1), new Vector2(cursorX, cursorY2),
                    Theme.ToU32(Theme.TextPrimary), 1f);
            }
        }
        else
        {
            dl.AddRectFilled(boxMin, boxMax, Theme.ToU32(SectionBg), showArrows ? 0f : 4f);
            dl.AddRect(boxMin, boxMax, Theme.ToU32(Theme.Border), showArrows ? 0f : 4f);

            var vSz = ImGuiNET.ImGui.CalcTextSize(displayVal);
            dl.AddText(
                new Vector2(boxMin.X + (SliderBoxW - vSz.X) * 0.5f, boxMin.Y + (boxH - vSz.Y) * 0.5f),
                Theme.ToU32(Theme.TextPrimary), displayVal);

            if (ImGuiNET.ImGui.IsMouseHoveringRect(boxMin, boxMax) &&
                ImGuiNET.ImGui.IsMouseClicked(ImGuiMouseButton.Left))
            {
                _editingSlider = label;
                _editBuffer = valueStr;
                _editFirstFrame = true;
            }
        }

        if (showArrows)
        {
            var arrowRMin = new Vector2(boxMax.X + 2f, pos.Y - 1f);
            var arrowRMax = new Vector2(arrowRMin.X + ArrowBtnW, arrowRMin.Y + boxH);

            bool arrowRHov = ImGuiNET.ImGui.IsMouseHoveringRect(arrowRMin, arrowRMax);
            dl.AddRectFilled(arrowRMin, arrowRMax,
                Theme.ToU32(arrowRHov ? Theme.BgHover : Theme.BgElevated), 4f, ImDrawFlags.RoundCornersRight);

            var aSzR = ImGuiNET.ImGui.CalcTextSize(">");
            dl.AddText(
                new Vector2(arrowRMin.X + (ArrowBtnW - aSzR.X) * 0.5f, arrowRMin.Y + (boxH - aSzR.Y) * 0.5f),
                Theme.ToU32(arrowRHov ? Theme.TextPrimary : Theme.TextSecondary), ">");

            if (arrowRHov && ImGuiNET.ImGui.IsMouseClicked(ImGuiMouseButton.Left))
            {
                value = MathF.Min(max, value + step);
                changed = true;
            }
        }

        var trackY = pos.Y + labelH + 8f;
        var trackW = contentW - SliderBoxPad;
        var knobH  = SliderKnobR * 2f;
        var totalH = labelH + 8f + knobH + 8f;

        var winPos2 = ImGuiNET.ImGui.GetWindowPos();
        ImGuiNET.ImGui.SetCursorPos(new Vector2(pos.X - winPos2.X, trackY - 2f - winPos2.Y));

        ImGuiNET.ImGui.InvisibleButton($"{label}_track", new Vector2(trackW, knobH + 4f));

        bool active  = ImGuiNET.ImGui.IsItemActive();
        bool hovered = ImGuiNET.ImGui.IsItemHovered();

        if (active)
        {
            var mouseX = ImGuiNET.ImGui.GetIO().MousePos.X;
            var t = (mouseX - pos.X) / trackW;
            t = MathF.Max(0f, MathF.Min(1f, t));
            var newVal = min + (max - min) * t;
            if (MathF.Abs(newVal - value) > 0.001f)
            {
                value = newVal;
                changed = true;
            }
        }

        var frac = MathF.Max(0f, MathF.Min(1f, (value - min) / (max - min)));

        var trkMin = new Vector2(pos.X, trackY + (knobH - SliderTrackH) * 0.5f);
        var trkMax = new Vector2(pos.X + trackW, trkMin.Y + SliderTrackH);
        var trkR   = SliderTrackH * 0.5f;

        dl.AddRectFilled(trkMin, trkMax, Theme.ToU32(SectionBg), trkR);
        dl.AddRect(trkMin, trkMax, TrackBorder, trkR);

        if (frac > 0.005f)
        {
            var fillMax = new Vector2(pos.X + trackW * frac, trkMax.Y);
            dl.AddRectFilled(trkMin, fillMax, TrackFillL, trkR);
            dl.AddRectFilledMultiColor(
                new Vector2(trkMin.X + trkR, trkMin.Y),
                new Vector2(fillMax.X, trkMax.Y),
                TrackFillL, TrackFillR, TrackFillR, TrackFillL);
        }

        var knobAnimId = $"{label}_knob";
        var knobT = GetToggleT(knobAnimId, active);

        var knobX  = pos.X + trackW * frac;
        var knobCY = trackY + knobH * 0.5f;
        var knobColor = (hovered || active) ? Theme.Hex(0xFFFFFF) : Theme.HexA(0xFFFFFF, 0.85f);

        var pillW = 4f;
        var pillH = 7f + 2f * knobT;
        var pillR = 3f;

        dl.AddRectFilled(
            new Vector2(knobX - pillW, knobCY - pillH),
            new Vector2(knobX + pillW, knobCY + pillH),
            Theme.ToU32(knobColor), pillR);

        var dragVelX = (knobX - _lastSliderKnobX) / MathF.Max(dt, 0.001f);

        if (active && changed && MathF.Abs(dragVelX) > 20f)
            SpawnSparks(new Vector2(knobX, knobCY), dragVelX, 3);

        _lastSliderKnobX = knobX;

        var fgDl = ImGuiNET.ImGui.GetForegroundDrawList();
        UpdateSparks(fgDl, dt);

        var winPosEnd = ImGuiNET.ImGui.GetWindowPos();
        ImGuiNET.ImGui.SetCursorPos(new Vector2(pos.X - winPosEnd.X, pos.Y + totalH - winPosEnd.Y));
        ImGuiNET.ImGui.Dummy(new Vector2(0f, 0f));

        ImGuiNET.ImGui.Spacing();
        return changed;
    }

    public static bool Combo(string label, ref int current, string items)
    {
        var parsed = items.Split('\0', StringSplitOptions.RemoveEmptyEntries);
        return Combo(label, ref current, parsed);
    }

    public static bool Combo(string label, ref int current, string[] items)
    {
        const float boxH     = 34f;
        const float boxW     = 160f;
        const float boxR     = 6f;
        const float pad      = 14f;
        const float stripH   = 36f;
        const float stripR   = 6f;
        const float closeW   = 38f;
        const float itemMin  = 72f;
        const float sepInset = 8f;

        ImGuiNET.ImGui.Spacing();

        var dl   = ImGuiNET.ImGui.GetWindowDrawList();
        var font = ImGuiNET.ImGui.GetFont();
        var fs   = font.FontSize;
        var cw   = ImGuiNET.ImGui.GetContentRegionAvail().X;
        var pos  = ImGuiNET.ImGui.GetCursorScreenPos();
        var changed = false;

        var dLabel  = label.Contains("##") ? label[..label.IndexOf("##")] : label;
        var pid     = $"{label}_p";
        bool isOpen = ImGuiNET.ImGui.IsPopupOpen(pid);

        if (!string.IsNullOrEmpty(dLabel))
        {
            ImGuiNET.ImGui.AlignTextToFramePadding();
            ImGuiNET.ImGui.TextUnformatted(dLabel);
        }

        ImGuiNET.ImGui.SameLine(cw - boxW);
        pos = ImGuiNET.ImGui.GetCursorScreenPos();

        var bMin = pos;
        var bMax = new Vector2(pos.X + boxW, pos.Y + boxH);

        ImGuiNET.ImGui.InvisibleButton($"{label}_b", new Vector2(boxW, boxH));
        bool bH = ImGuiNET.ImGui.IsItemHovered();
        if (bH && ImGuiNET.ImGui.IsMouseClicked(ImGuiMouseButton.Left))
            ImGuiNET.ImGui.OpenPopup(pid);

        dl.AddRectFilled(bMin, bMax, Theme.ToU32(SectionBg), boxR);
        dl.AddRect(bMin, bMax,
            Theme.ToU32(isOpen ? Theme.BorderActive : bH ? Theme.BorderHover : Theme.Border),
            boxR, ImDrawFlags.None, 1f);

        var vt = current >= 0 && current < items.Length ? items[current] : "";
        var vs = font.CalcTextSizeA(fs, float.MaxValue, 0f, vt);
        dl.AddText(font, fs,
            new Vector2(bMin.X + pad, bMin.Y + (boxH - vs.Y) * 0.5f),
            Theme.ToU32(Theme.TextPrimary), vt);

        var chev = isOpen ? "<" : ">";
        var chSz = font.CalcTextSizeA(fs, float.MaxValue, 0f, chev);
        dl.AddText(font, fs,
            new Vector2(bMax.X - pad - chSz.X, bMin.Y + (boxH - chSz.Y) * 0.5f),
            Theme.ToU32(isOpen ? Theme.TextPrimary : Theme.TextSecondary), chev);

        float maxTW = 0f;
        for (int i = 0; i < items.Length; i++)
        {
            var tw = font.CalcTextSizeA(fs, float.MaxValue, 0f, items[i]).X;
            if (tw > maxTW) maxTW = tw;
        }
        var iW     = MathF.Max(maxTW + 28f, itemMin);
        var totalW = closeW + iW * items.Length;
        var sepCol = Theme.ToU32(Theme.HexA(0xFFFFFF, 0.06f));

        ImGuiNET.ImGui.SetNextWindowPos(new Vector2(bMin.X, bMax.Y + 1f));
        ImGuiNET.ImGui.SetNextWindowSize(new Vector2(totalW, stripH));

        ImGuiNET.ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.Zero);
        ImGuiNET.ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, stripR);
        ImGuiNET.ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 1f);
        ImGuiNET.ImGui.PushStyleColor(ImGuiCol.PopupBg, Theme.BgElevated);
        ImGuiNET.ImGui.PushStyleColor(ImGuiCol.Border, Theme.HexA(0xFFFFFF, 0.10f));

        if (ImGuiNET.ImGui.BeginPopup(pid,
            ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize))
        {
            var p  = ImGuiNET.ImGui.GetWindowDrawList();
            var wP = ImGuiNET.ImGui.GetWindowPos();
            ImGuiNET.ImGui.Dummy(new Vector2(totalW, stripH));

            var xMin = wP;
            var xMax = new Vector2(wP.X + closeW, wP.Y + stripH);
            bool xHov = ImGuiNET.ImGui.IsMouseHoveringRect(xMin, xMax);

            if (xHov)
                p.AddRectFilled(xMin, xMax, Theme.ToU32(Theme.BgHover), stripR, ImDrawFlags.RoundCornersLeft);

            var xTs = font.CalcTextSizeA(fs, float.MaxValue, 0f, "X");
            p.AddText(font, fs,
                new Vector2(xMin.X + (closeW - xTs.X) * 0.5f, xMin.Y + (stripH - xTs.Y) * 0.5f),
                Theme.ToU32(xHov ? Theme.TextPrimary : Theme.TextSecondary), "X");

            if (xHov && ImGuiNET.ImGui.IsMouseClicked(ImGuiMouseButton.Left))
                ImGuiNET.ImGui.CloseCurrentPopup();

            p.AddLine(new Vector2(xMax.X, wP.Y + sepInset), new Vector2(xMax.X, wP.Y + stripH - sepInset), sepCol);

            for (int i = 0; i < items.Length; i++)
            {
                var ox   = wP.X + closeW + iW * i;
                var oMin = new Vector2(ox, wP.Y);
                var oMax = new Vector2(ox + iW, wP.Y + stripH);
                bool oHov = ImGuiNET.ImGui.IsMouseHoveringRect(oMin, oMax);
                bool sel  = i == current;

                var cf = i == items.Length - 1 ? ImDrawFlags.RoundCornersRight : ImDrawFlags.None;

                if (sel)
                    p.AddRectFilled(oMin, oMax, Theme.ToU32(Theme.HexA(0x9B30FF, 0.25f)), stripR, cf);
                else if (oHov)
                    p.AddRectFilled(oMin, oMax, Theme.ToU32(Theme.BgHover), stripR, cf);

                var ts = font.CalcTextSizeA(fs, float.MaxValue, 0f, items[i]);
                p.AddText(font, fs,
                    new Vector2(oMin.X + (iW - ts.X) * 0.5f, oMin.Y + (stripH - ts.Y) * 0.5f),
                    Theme.ToU32(sel ? Theme.Cyan : Theme.TextPrimary), items[i]);

                if (i < items.Length - 1)
                    p.AddLine(new Vector2(oMax.X, wP.Y + sepInset), new Vector2(oMax.X, wP.Y + stripH - sepInset), sepCol);

                if (oHov && ImGuiNET.ImGui.IsMouseClicked(ImGuiMouseButton.Left))
                {
                    current = i;
                    changed = true;
                    ImGuiNET.ImGui.CloseCurrentPopup();
                }
            }

            ImGuiNET.ImGui.EndPopup();
        }

        ImGuiNET.ImGui.PopStyleColor(2);
        ImGuiNET.ImGui.PopStyleVar(3);

        ImGuiNET.ImGui.Spacing();
        return changed;
    }

    public static void DrawGradientText(ImDrawListPtr dl, ImFontPtr font, float fontSize,
        Vector2 pos, string text, Vector4 colorStart, Vector4 colorMid, Vector4 colorEnd,
        float shear = 0f)
    {
        if (string.IsNullOrEmpty(text)) return;

        var x = pos.X;
        var lastIdx = text.Length - 1;
        var charH = font.CalcTextSizeA(fontSize, float.MaxValue, 0f, "A").Y;

        for (int i = 0; i < text.Length; i++)
        {
            var t = lastIdx > 0 ? (float)i / lastIdx : 0f;

            Vector4 color;
            if (t < 0.5f)
            {
                var lt = t * 2f;
                color = colorStart + (colorMid - colorStart) * lt;
            }
            else
            {
                var lt = (t - 0.5f) * 2f;
                color = colorMid + (colorEnd - colorMid) * lt;
            }

            var ch = text[i].ToString();
            var charW = font.CalcTextSizeA(fontSize, float.MaxValue, 0f, ch).X;

            if (shear != 0f)
            {
                var shearOffset = shear * charH;
                dl.AddText(font, fontSize, new Vector2(x + shearOffset, pos.Y), Theme.ToU32(color), ch);
            }
            else
            {
                dl.AddText(font, fontSize, new Vector2(x, pos.Y), Theme.ToU32(color), ch);
            }

            x += charW;
        }
    }
}