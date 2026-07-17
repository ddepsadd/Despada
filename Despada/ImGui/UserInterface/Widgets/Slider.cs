using System.Numerics;
using ImGuiNET;

namespace Despada.ImGui.UserInterface;

public static partial class Widgets
{
    private const float SliderTrackH = 6f;
    private const float SliderKnobR  = 7f;
    private const float SliderBoxW   = 72f;
    private const float SliderBoxPad = 16f;
    private const float ArrowBtnW    = 18f;

    private static readonly uint TrackBorder = Theme.ToU32(Theme.Border);
    private static readonly uint TrackFillL  = Theme.ToU32(Theme.Violet);
    private static readonly uint TrackFillR  = Theme.ToU32(Theme.Cyan);

    private static string? _editingSlider;
    private static string  _editBuffer = "";
    private static float   _lastSliderKnobX;

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
        var groupRight = pos.X + contentW - SliderBoxPad;
        var groupLeft  = groupRight - totalBoxW;

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
}