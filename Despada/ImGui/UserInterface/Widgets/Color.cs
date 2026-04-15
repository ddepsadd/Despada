using System.Globalization;
using System.Numerics;
using ImGuiNET;

namespace Despada.ImGui.UserInterface;

public static partial class Widgets
{
    private sealed class ColorPickerState
    {
        public string HexBuffer = "FFFFFFFF";
        public bool HexDirty;
    }

    private static readonly Dictionary<string, ColorPickerState> _colorPickerState = new();

    public static bool ColorPickerRow(string label, ref Vector4 color)
    {
        const float previewW      = 40f;
        const float previewH      = 20f;
        const float popupW        = 430f;
        const float popupPad      = 12f;

        const float svSize        = 220f;
        const float hueW          = 18f;
        const float hueH          = 220f;
        const float alphaH        = 16f;

        const float rightPanelW   = 120f;
        const float bigPreviewW   = 92f;
        const float bigPreviewH   = 92f;
        const float topGap        = 12f;
        const float leftBlockW    = svSize + 12f + hueW;
        const float topBlockH     = svSize;

        const float presetSize    = 22f;
        const float presetGap     = 6f;
        const int   presetCols    = 6;

        ImGuiNET.ImGui.Spacing();

        var changed   = false;
        var contentW  = ImGuiNET.ImGui.GetContentRegionAvail().X;
        var popupId   = $"##{label}_popup";
        var previewId = $"##{label}_preview";
        var stateId   = $"##{label}_state";

        var state = GetColorPickerState(stateId);
        SyncHexBufferFromColor(state, color);

        ImGuiNET.ImGui.TextUnformatted(label);
        ImGuiNET.ImGui.SameLine(contentW - previewW);

        ImGuiNET.ImGui.InvisibleButton(previewId, new Vector2(previewW, previewH));

        bool hovered = ImGuiNET.ImGui.IsItemHovered();
        bool clicked = hovered && ImGuiNET.ImGui.IsMouseClicked(ImGuiMouseButton.Left);

        if (clicked)
        {
            SyncHexBufferFromColor(state, color, force: true);
            ImGuiNET.ImGui.OpenPopup(popupId);
        }

        var previewMin = ImGuiNET.ImGui.GetItemRectMin();
        var previewMax = ImGuiNET.ImGui.GetItemRectMax();
        var dl = ImGuiNET.ImGui.GetWindowDrawList();

        DrawColorPreview(dl, previewMin, previewMax, color, hovered ? 0.92f : 1f, 4f);

        ImGuiNET.ImGui.SetNextWindowSize(new Vector2(popupW, 0f), ImGuiCond.Appearing);
        ImGuiNET.ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(popupPad, popupPad));
        ImGuiNET.ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 10f);
        ImGuiNET.ImGui.PushStyleColor(ImGuiCol.PopupBg, Theme.BgElevated);
        ImGuiNET.ImGui.PushStyleColor(ImGuiCol.Border, Theme.HexA(0xFFFFFF, 0.10f));

        if (ImGuiNET.ImGui.BeginPopup(popupId))
        {
            var editColor = color;
            RgbToHsv(editColor, out var h, out var s, out var v);
            var a = editColor.W;

            DrawPresetPalette(label, ref editColor, ref changed, presetSize, presetGap, presetCols);

            if (changed)
            {
                RgbToHsv(editColor, out h, out s, out v);
                a = editColor.W;
                SyncHexBufferFromColor(state, editColor, force: true);
            }

            ImGuiNET.ImGui.Spacing();
            ImGuiNET.ImGui.Separator();
            ImGuiNET.ImGui.Spacing();

            var topOrigin = ImGuiNET.ImGui.GetCursorScreenPos();
            var leftOrigin = topOrigin;
            var rightOrigin = new Vector2(topOrigin.X + leftBlockW + topGap, topOrigin.Y);

            var svMin = leftOrigin;
            var hueMin = new Vector2(leftOrigin.X + svSize + 12f, leftOrigin.Y);

            if (DrawSvSquare(label, svMin, svSize, h, ref s, ref v))
                changed = true;

            if (DrawHueBar(label, hueMin, hueW, hueH, ref h))
                changed = true;

            var composed = HsvToRgb(h, s, v, a);
            editColor.X = composed.X;
            editColor.Y = composed.Y;
            editColor.Z = composed.Z;
            editColor.W = a;

            DrawBigPreviewCard(rightOrigin, bigPreviewW, bigPreviewH, editColor);

            ImGuiNET.ImGui.SetCursorScreenPos(new Vector2(topOrigin.X, topOrigin.Y + topBlockH));
            ImGuiNET.ImGui.Dummy(new Vector2(leftBlockW + topGap + rightPanelW, 0f));

            ImGuiNET.ImGui.Spacing();

            if (DrawAlphaSlider(label, alphaH, ref editColor))
            {
                changed = true;
                a = editColor.W;
                var rgbFromHsv = HsvToRgb(h, s, v, a);
                editColor = rgbFromHsv;
            }

            ImGuiNET.ImGui.Spacing();
            ImGuiNET.ImGui.Separator();
            ImGuiNET.ImGui.Spacing();

            DrawRgbaEditors(label, ref editColor, ref changed);

            if (changed)
            {
                RgbToHsv(editColor, out h, out s, out v);
                a = editColor.W;
                SyncHexBufferFromColor(state, editColor, force: true);
            }

            DrawHexEditor(label, state, ref editColor, ref changed);

            if (changed)
            {
                color = editColor;
                SyncHexBufferFromColor(state, color, force: true);
            }

            ImGuiNET.ImGui.EndPopup();
        }

        ImGuiNET.ImGui.PopStyleColor(2);
        ImGuiNET.ImGui.PopStyleVar(2);

        ImGuiNET.ImGui.Spacing();
        return changed;
    }

    private static ColorPickerState GetColorPickerState(string id)
    {
        if (!_colorPickerState.TryGetValue(id, out var state))
        {
            state = new ColorPickerState();
            _colorPickerState[id] = state;
        }

        return state;
    }

    private static void SyncHexBufferFromColor(ColorPickerState state, Vector4 color, bool force = false)
    {
        if (force || !state.HexDirty)
            state.HexBuffer = ColorToHex(color, includeAlpha: true);
    }

    private static void DrawPresetPalette(string label, ref Vector4 color, ref bool changed,
        float cellSize, float gap, int cols)
    {
        Vector4[] presets =
        [
            Theme.Green,
            Theme.Red,
            Theme.Cyan,
            Theme.Violet,
            Theme.Magenta,
            Theme.Yellow,
            Theme.Hex(0xFFFFFF),
            Theme.Hex(0x000000),
            Theme.Hex(0x00FF00),
            Theme.Hex(0x00BFFF),
            Theme.Hex(0xFF8800),
            Theme.Hex(0xFF66CC)
        ];

        ImGuiNET.ImGui.TextColored(Theme.TextSecondary, "Presets");

        for (int i = 0; i < presets.Length; i++)
        {
            if (i > 0 && i % cols != 0)
                ImGuiNET.ImGui.SameLine(0f, gap);

            ImGuiNET.ImGui.InvisibleButton($"##{label}_preset_{i}", new Vector2(cellSize, cellSize));

            var min = ImGuiNET.ImGui.GetItemRectMin();
            var max = ImGuiNET.ImGui.GetItemRectMax();
            var dl  = ImGuiNET.ImGui.GetWindowDrawList();
            bool hovered = ImGuiNET.ImGui.IsItemHovered();

            var draw = hovered
                ? new Vector4(
                    MathF.Min(presets[i].X + 0.08f, 1f),
                    MathF.Min(presets[i].Y + 0.08f, 1f),
                    MathF.Min(presets[i].Z + 0.08f, 1f),
                    presets[i].W)
                : presets[i];

            DrawColorPreview(dl, min, max, draw, 1f, 5f);

            if (ColorsClose(color, presets[i]))
            {
                var c = new Vector2((min.X + max.X) * 0.5f, (min.Y + max.Y) * 0.5f);
                dl.AddCircleFilled(c, 3f, Theme.ToU32(Theme.Hex(0xFFFFFF)));
                dl.AddCircle(c, 4.5f, Theme.ToU32(Theme.HexA(0x000000, 0.50f)), 0, 1.5f);
            }

            if (hovered && ImGuiNET.ImGui.IsMouseClicked(ImGuiMouseButton.Left))
            {
                color = presets[i];
                changed = true;
            }
        }
    }

    private static bool DrawSvSquare(string label, Vector2 topLeft, float size, float h,
        ref float s, ref float v)
    {
        var id = $"##{label}_sv_square";

        ImGuiNET.ImGui.SetCursorScreenPos(topLeft);
        ImGuiNET.ImGui.InvisibleButton(id, new Vector2(size, size));

        var min = ImGuiNET.ImGui.GetItemRectMin();
        var max = ImGuiNET.ImGui.GetItemRectMax();
        var dl  = ImGuiNET.ImGui.GetWindowDrawList();

        bool changed = false;
        bool active = ImGuiNET.ImGui.IsItemActive();

        if (active)
        {
            var mouse = ImGuiNET.ImGui.GetIO().MousePos;
            s = Math.Clamp((mouse.X - min.X) / size, 0f, 1f);
            v = Math.Clamp(1f - (mouse.Y - min.Y) / size, 0f, 1f);
            changed = true;
        }

        var hueColor = HsvToRgb(h, 1f, 1f, 1f);

        dl.AddRectFilledMultiColor(
            min,
            max,
            Theme.ToU32(Theme.Hex(0xFFFFFF)),
            Theme.ToU32(hueColor),
            Theme.ToU32(hueColor),
            Theme.ToU32(Theme.Hex(0xFFFFFF)));

        dl.AddRectFilledMultiColor(
            min,
            max,
            Theme.ToU32(Theme.HexA(0x000000, 0f)),
            Theme.ToU32(Theme.HexA(0x000000, 0f)),
            Theme.ToU32(Theme.HexA(0x000000, 1f)),
            Theme.ToU32(Theme.HexA(0x000000, 1f)));

        dl.AddRect(min, max, Theme.ToU32(Theme.Border), 8f, ImDrawFlags.None, 1f);

        var cx = min.X + s * size;
        var cy = min.Y + (1f - v) * size;
        var c = new Vector2(cx, cy);

        dl.AddCircleFilled(c, 6f, Theme.ToU32(Theme.HexA(0x000000, 0.55f)));
        dl.AddCircle(c, 6f, Theme.ToU32(Theme.Hex(0xFFFFFF)), 0, 2f);

        return changed;
    }

    private static bool DrawHueBar(string label, Vector2 topLeft, float width, float height, ref float h)
    {
        var id = $"##{label}_hue_bar";

        ImGuiNET.ImGui.SetCursorScreenPos(topLeft);
        ImGuiNET.ImGui.InvisibleButton(id, new Vector2(width, height));

        var min = ImGuiNET.ImGui.GetItemRectMin();
        var max = ImGuiNET.ImGui.GetItemRectMax();
        var dl  = ImGuiNET.ImGui.GetWindowDrawList();

        bool changed = false;
        bool active = ImGuiNET.ImGui.IsItemActive();

        if (active)
        {
            var mouseY = ImGuiNET.ImGui.GetIO().MousePos.Y;
            h = Math.Clamp((mouseY - min.Y) / height, 0f, 1f);
            changed = true;
        }

        const int segments = 6;
        float segH = height / segments;

        Vector4[] hues =
        [
            Theme.Hex(0xFF0000),
            Theme.Hex(0xFFFF00),
            Theme.Hex(0x00FF00),
            Theme.Hex(0x00FFFF),
            Theme.Hex(0x0000FF),
            Theme.Hex(0xFF00FF),
            Theme.Hex(0xFF0000)
        ];

        for (int i = 0; i < segments; i++)
        {
            float y1 = min.Y + segH * i;
            float y2 = y1 + segH;

            dl.AddRectFilledMultiColor(
                new Vector2(min.X, y1),
                new Vector2(max.X, y2),
                Theme.ToU32(hues[i]),
                Theme.ToU32(hues[i]),
                Theme.ToU32(hues[i + 1]),
                Theme.ToU32(hues[i + 1]));
        }

        dl.AddRect(min, max, Theme.ToU32(Theme.Border), 6f, ImDrawFlags.None, 1f);

        float markerY = min.Y + h * height;
        dl.AddRectFilled(
            new Vector2(min.X - 2f, markerY - 2f),
            new Vector2(max.X + 2f, markerY + 2f),
            Theme.ToU32(Theme.Hex(0xFFFFFF)),
            2f);

        return changed;
    }

    private static bool DrawAlphaSlider(string label, float height, ref Vector4 color)
    {
        var width = 220f;

        ImGuiNET.ImGui.TextColored(Theme.TextSecondary, $"Alpha: {ToByte01(color.W)}");

        var pos = ImGuiNET.ImGui.GetCursorScreenPos();
        ImGuiNET.ImGui.InvisibleButton($"##{label}_alpha_slider", new Vector2(width, height));

        var min = ImGuiNET.ImGui.GetItemRectMin();
        var max = ImGuiNET.ImGui.GetItemRectMax();
        var dl  = ImGuiNET.ImGui.GetWindowDrawList();

        bool changed = false;
        bool active = ImGuiNET.ImGui.IsItemActive();

        if (active)
        {
            var mouseX = ImGuiNET.ImGui.GetIO().MousePos.X;
            color.W = Math.Clamp((mouseX - min.X) / width, 0f, 1f);
            changed = true;
        }

        DrawAlphaBackground(dl, min, max, 6f);

        var left  = new Vector4(color.X, color.Y, color.Z, 0f);
        var right = new Vector4(color.X, color.Y, color.Z, 1f);

        dl.AddRectFilledMultiColor(
            min,
            max,
            Theme.ToU32(left),
            Theme.ToU32(right),
            Theme.ToU32(right),
            Theme.ToU32(left));

        dl.AddRect(min, max, Theme.ToU32(Theme.Border), 6f, ImDrawFlags.None, 1f);

        float markerX = min.X + color.W * width;
        dl.AddRectFilled(
            new Vector2(markerX - 2f, min.Y - 2f),
            new Vector2(markerX + 2f, max.Y + 2f),
            Theme.ToU32(Theme.Hex(0xFFFFFF)),
            2f);

        ImGuiNET.ImGui.SetCursorScreenPos(new Vector2(pos.X, pos.Y + height));
        ImGuiNET.ImGui.Dummy(Vector2.Zero);

        return changed;
    }

    private static void DrawBigPreviewCard(Vector2 topLeft, float width, float height, Vector4 color)
    {
        var min = topLeft;
        var max = new Vector2(topLeft.X + width, topLeft.Y + height);
        var dl  = ImGuiNET.ImGui.GetWindowDrawList();

        DrawColorPreview(dl, min, max, color, 1f, 8f);
        dl.AddRect(min, max, Theme.ToU32(Theme.Border), 8f, ImDrawFlags.None, 1f);
    }

    private static void DrawRgbaEditors(string label, ref Vector4 color, ref bool changed)
    {
        int r = ToByte01(color.X);
        int g = ToByte01(color.Y);
        int b = ToByte01(color.Z);
        int a = ToByte01(color.W);

        ImGuiNET.ImGui.TextColored(Theme.TextSecondary, "RGBA");

        ImGuiNET.ImGui.SetNextItemWidth(56f);
        if (ImGuiNET.ImGui.InputInt($"R##{label}_r", ref r, 0, 0))
        {
            color.X = Math.Clamp(r, 0, 255) / 255f;
            changed = true;
        }

        ImGuiNET.ImGui.SameLine();
        ImGuiNET.ImGui.SetNextItemWidth(56f);
        if (ImGuiNET.ImGui.InputInt($"G##{label}_g", ref g, 0, 0))
        {
            color.Y = Math.Clamp(g, 0, 255) / 255f;
            changed = true;
        }

        ImGuiNET.ImGui.SameLine();
        ImGuiNET.ImGui.SetNextItemWidth(56f);
        if (ImGuiNET.ImGui.InputInt($"B##{label}_b", ref b, 0, 0))
        {
            color.Z = Math.Clamp(b, 0, 255) / 255f;
            changed = true;
        }

        ImGuiNET.ImGui.SameLine();
        ImGuiNET.ImGui.SetNextItemWidth(56f);
        if (ImGuiNET.ImGui.InputInt($"A##{label}_a", ref a, 0, 0))
        {
            color.W = Math.Clamp(a, 0, 255) / 255f;
            changed = true;
        }
    }

    private static void DrawHexEditor(string label, ColorPickerState state, ref Vector4 color, ref bool changed)
    {
        ImGuiNET.ImGui.TextColored(Theme.TextSecondary, "HEX");
        ImGuiNET.ImGui.SameLine();

        ImGuiNET.ImGui.SetNextItemWidth(140f);
        if (ImGuiNET.ImGui.InputText($"##{label}_hex_input", ref state.HexBuffer, 9,
                ImGuiInputTextFlags.CharsHexadecimal | ImGuiInputTextFlags.CharsUppercase))
        {
            state.HexDirty = true;
        }

        ImGuiNET.ImGui.SameLine();

        if (ImGuiNET.ImGui.Button($"Apply##{label}_hex_apply"))
        {
            if (TryParseHexColor(state.HexBuffer, out var parsed))
            {
                color = parsed;
                state.HexBuffer = ColorToHex(color, includeAlpha: true);
                state.HexDirty = false;
                changed = true;
            }
        }

        ImGuiNET.ImGui.SameLine();

        if (ImGuiNET.ImGui.Button($"Copy##{label}_hex_copy"))
            ImGuiNET.ImGui.SetClipboardText(ColorToHex(color, includeAlpha: true));
    }

    private static void DrawColorPreview(ImDrawListPtr dl, Vector2 min, Vector2 max, Vector4 color, float rgbMul, float rounding)
    {
        DrawAlphaBackground(dl, min, max, rounding);

        var tinted = new Vector4(
            Math.Clamp(color.X * rgbMul, 0f, 1f),
            Math.Clamp(color.Y * rgbMul, 0f, 1f),
            Math.Clamp(color.Z * rgbMul, 0f, 1f),
            color.W);

        dl.AddRectFilled(min, max, Theme.ToU32(tinted), rounding);
    }

    private static void DrawAlphaBackground(ImDrawListPtr dl, Vector2 min, Vector2 max, float rounding)
    {
        const float checker = 6f;

        dl.AddRectFilled(min, max, Theme.ToU32(Theme.Hex(0x1A1A1A)), rounding);

        for (float y = min.Y; y < max.Y; y += checker)
        {
            for (float x = min.X; x < max.X; x += checker)
            {
                bool odd = (((int)((x - min.X) / checker)) + ((int)((y - min.Y) / checker))) % 2 == 0;
                if (!odd) continue;

                var cMin = new Vector2(x, y);
                var cMax = new Vector2(
                    MathF.Min(x + checker, max.X),
                    MathF.Min(y + checker, max.Y));

                dl.AddRectFilled(cMin, cMax, Theme.ToU32(Theme.Hex(0x2A2A2A)));
            }
        }
    }

    private static void RgbToHsv(Vector4 rgb, out float h, out float s, out float v)
    {
        float r = rgb.X;
        float g = rgb.Y;
        float b = rgb.Z;

        float max = MathF.Max(r, MathF.Max(g, b));
        float min = MathF.Min(r, MathF.Min(g, b));
        float delta = max - min;

        v = max;

        if (max <= 0f)
        {
            s = 0f;
            h = 0f;
            return;
        }

        s = delta / max;

        if (delta <= 0.0001f)
        {
            h = 0f;
            return;
        }

        if (max == r)
            h = (g - b) / delta;
        else if (max == g)
            h = 2f + (b - r) / delta;
        else
            h = 4f + (r - g) / delta;

        h /= 6f;
        if (h < 0f)
            h += 1f;
    }

    private static Vector4 HsvToRgb(float h, float s, float v, float a = 1f)
    {
        h = h - MathF.Floor(h);
        s = Math.Clamp(s, 0f, 1f);
        v = Math.Clamp(v, 0f, 1f);

        if (s <= 0f)
            return new Vector4(v, v, v, a);

        float hh = h * 6f;
        int sector = (int)MathF.Floor(hh);
        float f = hh - sector;

        float p = v * (1f - s);
        float q = v * (1f - s * f);
        float t = v * (1f - s * (1f - f));

        return sector switch
        {
            0 => new Vector4(v, t, p, a),
            1 => new Vector4(q, v, p, a),
            2 => new Vector4(p, v, t, a),
            3 => new Vector4(p, q, v, a),
            4 => new Vector4(t, p, v, a),
            _ => new Vector4(v, p, q, a),
        };
    }

    private static string ColorToHex(Vector4 color, bool includeAlpha)
    {
        int r = ToByte01(color.X);
        int g = ToByte01(color.Y);
        int b = ToByte01(color.Z);
        int a = ToByte01(color.W);

        return includeAlpha
            ? $"{r:X2}{g:X2}{b:X2}{a:X2}"
            : $"{r:X2}{g:X2}{b:X2}";
    }

    private static bool TryParseHexColor(string input, out Vector4 color)
    {
        color = default;

        if (string.IsNullOrWhiteSpace(input))
            return false;

        var hex = input.Trim().TrimStart('#');

        if (hex.Length is not (6 or 8))
            return false;

        if (!uint.TryParse(hex, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out _))
            return false;

        byte r = byte.Parse(hex[..2], NumberStyles.HexNumber, CultureInfo.InvariantCulture);
        byte g = byte.Parse(hex[2..4], NumberStyles.HexNumber, CultureInfo.InvariantCulture);
        byte b = byte.Parse(hex[4..6], NumberStyles.HexNumber, CultureInfo.InvariantCulture);
        byte a = hex.Length == 8
            ? byte.Parse(hex[6..8], NumberStyles.HexNumber, CultureInfo.InvariantCulture)
            : (byte)255;

        color = new Vector4(
            r / 255f,
            g / 255f,
            b / 255f,
            a / 255f);

        return true;
    }

    private static int ToByte01(float value)
    {
        return (int)MathF.Round(Math.Clamp(value, 0f, 1f) * 255f);
    }

    private static bool ColorsClose(Vector4 a, Vector4 b)
    {
        return MathF.Abs(a.X - b.X) < 0.001f &&
               MathF.Abs(a.Y - b.Y) < 0.001f &&
               MathF.Abs(a.Z - b.Z) < 0.001f &&
               MathF.Abs(a.W - b.W) < 0.001f;
    }
}