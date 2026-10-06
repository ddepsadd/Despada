// SPDX-FileCopyrightText: 2026 ddepsadd <https://github.com/ddepsadd>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Globalization;
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

    private static readonly StateStore<float> _sliderKnobX = new();

    private static uint   _editSliderId;
    private static int    _editStartFrame;
    private static string _editBuffer = "";

    /// <param name="decimals">Digits after the point — used for display, input and rounding.</param>
    /// <param name="step">Arrow button step; 0 hides the arrows.</param>
    public static bool SliderFloat(string label, ref float value, float min, float max,
        int decimals = 1, string suffix = "", float step = 0f)
    {
        double v = value;
        if (!SliderCore(label, ref v, min, max, decimals, suffix, step))
            return false;
        value = (float)v;
        return true;
    }

    /// <param name="step">Arrow button step; 0 hides the arrows.</param>
    public static bool SliderInt(string label, ref int value, int min, int max,
        string suffix = "", int step = 1)
    {
        double v = value;
        if (!SliderCore(label, ref v, min, max, 0, suffix, step))
            return false;
        value = (int)v;
        return true;
    }

    private static bool SliderCore(string label, ref double value, double min, double max,
        int decimals, string suffix, double step)
    {
        ImGuiNET.ImGui.Spacing();
        ImGuiNET.ImGui.PushID(label);

        var dl       = ImGuiNET.ImGui.GetWindowDrawList();
        var contentW = ImGuiNET.ImGui.GetContentRegionAvail().X;
        var pos      = ImGuiNET.ImGui.GetCursorScreenPos();
        var dt       = ImGuiNET.ImGui.GetIO().DeltaTime;
        var changed  = false;

        var labelH = ImGuiNET.ImGui.GetTextLineHeightWithSpacing();
        var boxH   = labelH + 6f;
        var knobH  = SliderKnobR * 2f;
        var trackY = pos.Y + labelH + 8f;
        var trackW = contentW - SliderBoxPad;
        var totalH = labelH + 8f + knobH + 8f;

        dl.AddText(pos, ImGuiNET.ImGui.GetColorU32(ImGuiCol.Text), DisplayLabel(label).Trim());

        var showArrows = step > 0;
        var totalBoxW  = SliderBoxW + (showArrows ? ArrowBtnW * 2f + 4f : 0f);
        var groupLeft  = pos.X + contentW - SliderBoxPad - totalBoxW;
        var boxX       = groupLeft + (showArrows ? ArrowBtnW + 2f : 0f);
        var boxMin     = new Vector2(boxX, pos.Y - 1f);
        var boxMax     = new Vector2(boxX + SliderBoxW, boxMin.Y + boxH);
        var boxRound   = showArrows ? 0f : 4f;

        if (showArrows)
        {
            var aMin = new Vector2(groupLeft, pos.Y - 1f);
            if (DrawArrowButton(dl, "##dec", "<", aMin, boxH, ImDrawFlags.RoundCornersLeft))
            {
                value = SnapValue(value - step, min, max, decimals);
                changed = true;
            }
        }

        var boxId = ImGuiNET.ImGui.GetID("##box");
        if (_editSliderId == boxId)
        {
            if (DrawSliderEditor(boxMin, boxH, min, max, decimals, ref value))
                changed = true;
        }
        else
        {
            if (ButtonAt("##box", boxMin, new Vector2(SliderBoxW, boxH), out var boxHov))
            {
                _editSliderId   = boxId;
                _editStartFrame = ImGuiNET.ImGui.GetFrameCount();
                _editBuffer     = FormatValue(value, decimals);
            }

            dl.AddRectFilled(boxMin, boxMax, Theme.ToU32(SectionBg), boxRound);
            dl.AddRect(boxMin, boxMax, Theme.ToU32(boxHov ? Theme.BorderHover : Theme.Border), boxRound);

            var valueStr   = FormatValue(value, decimals);
            var displayVal = string.IsNullOrEmpty(suffix) ? valueStr : $"{valueStr} {suffix}";
            var vSz = ImGuiNET.ImGui.CalcTextSize(displayVal);
            dl.AddText(
                new Vector2(boxMin.X + (SliderBoxW - vSz.X) * 0.5f, boxMin.Y + (boxH - vSz.Y) * 0.5f),
                Theme.ToU32(Theme.TextPrimary), displayVal);
        }

        if (showArrows)
        {
            var aMin = new Vector2(boxMax.X + 2f, pos.Y - 1f);
            if (DrawArrowButton(dl, "##inc", ">", aMin, boxH, ImDrawFlags.RoundCornersRight))
            {
                value = SnapValue(value + step, min, max, decimals);
                changed = true;
            }
        }

        ButtonAt("##track", new Vector2(pos.X, trackY - 2f), new Vector2(trackW, knobH + 4f), out var hovered);
        var trackId = ImGuiNET.ImGui.GetItemID();
        bool active = ImGuiNET.ImGui.IsItemActive();

        if (active)
        {
            var t = Math.Clamp((ImGuiNET.ImGui.GetIO().MousePos.X - pos.X) / trackW, 0f, 1f);
            var newVal = SnapValue(min + (max - min) * t, min, max, decimals);
            if (newVal != value)
            {
                value = newVal;
                changed = true;
            }
        }

        var frac = max > min ? (float)Math.Clamp((value - min) / (max - min), 0.0, 1.0) : 0f;

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

        var knobT = Animate(ImGuiNET.ImGui.GetID("##knob"), active);

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

        ref var lastKnobX = ref _sliderKnobX.Get(trackId, knobX);
        var dragVelX = (knobX - lastKnobX) / MathF.Max(dt, 0.001f);
        lastKnobX = knobX;

        if (active && changed && MathF.Abs(dragVelX) > 20f)
            SpawnSparks(new Vector2(knobX, knobCY), dragVelX, 3);

        // Everything above was placed absolutely; reserve the whole block as one item.
        ImGuiNET.ImGui.SetCursorScreenPos(pos);
        ImGuiNET.ImGui.Dummy(new Vector2(contentW, totalH));

        ImGuiNET.ImGui.PopID();
        ImGuiNET.ImGui.Spacing();
        return changed;
    }

    private static bool DrawArrowButton(ImDrawListPtr dl, string id, string glyph, Vector2 min, float h,
        ImDrawFlags corners)
    {
        var pressed = ButtonAt(id, min, new Vector2(ArrowBtnW, h), out var hov);
        var max = new Vector2(min.X + ArrowBtnW, min.Y + h);

        dl.AddRectFilled(min, max, Theme.ToU32(hov ? Theme.BgHover : Theme.BgElevated), 4f, corners);

        var sz = ImGuiNET.ImGui.CalcTextSize(glyph);
        dl.AddText(
            new Vector2(min.X + (ArrowBtnW - sz.X) * 0.5f, min.Y + (h - sz.Y) * 0.5f),
            Theme.ToU32(hov ? Theme.TextPrimary : Theme.TextSecondary), glyph);

        return pressed;
    }

    /// <summary>Inline text editor over the value box. Commits on Enter / click-away, Esc cancels.</summary>
    private static bool DrawSliderEditor(Vector2 boxMin, float boxH, double min, double max, int decimals,
        ref double value)
    {
        var maxLen = (uint)Math.Max(FormatValue(min, decimals).Length, FormatValue(max, decimals).Length);
        var frame  = ImGuiNET.ImGui.GetFrameCount();

        var fontH = ImGuiNET.ImGui.GetFontSize();
        ImGuiNET.ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new Vector2(8f, MathF.Max(0f, (boxH - fontH) * 0.5f)));
        ImGuiNET.ImGui.PushStyleVar(ImGuiStyleVar.FrameBorderSize, 1f);
        ImGuiNET.ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding, 0f);
        ImGuiNET.ImGui.PushStyleColor(ImGuiCol.FrameBg, SectionBg);
        ImGuiNET.ImGui.PushStyleColor(ImGuiCol.Border, Theme.BorderActive);

        ImGuiNET.ImGui.SetCursorScreenPos(boxMin);
        ImGuiNET.ImGui.SetNextItemWidth(SliderBoxW);
        if (frame == _editStartFrame)
            ImGuiNET.ImGui.SetKeyboardFocusHere();

        ImGuiNET.ImGui.InputText("##edit", ref _editBuffer, maxLen,
            ImGuiInputTextFlags.CharsDecimal | ImGuiInputTextFlags.AutoSelectAll);

        // Focus lands a frame or two after SetKeyboardFocusHere — don't treat that gap as "lost focus".
        bool finished = ImGuiNET.ImGui.IsItemDeactivated()
                     || (frame - _editStartFrame > 2 && !ImGuiNET.ImGui.IsItemActive());

        ImGuiNET.ImGui.PopStyleColor(2);
        ImGuiNET.ImGui.PopStyleVar(3);

        if (!finished)
            return false;

        _editSliderId = 0;

        // Esc reverts the buffer before deactivating, so it commits the original value — a no-op.
        if (!double.TryParse(_editBuffer.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture,
                out var parsed))
            return false;

        var snapped = SnapValue(parsed, min, max, decimals);
        if (snapped == value)
            return false;

        value = snapped;
        return true;
    }

    private static double SnapValue(double v, double min, double max, int decimals)
        => Math.Clamp(Math.Round(v, decimals, MidpointRounding.AwayFromZero), min, max);

    private static string FormatValue(double v, int decimals)
        => v.ToString("F" + decimals, CultureInfo.InvariantCulture);
}
