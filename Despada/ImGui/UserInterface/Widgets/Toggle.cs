using System.Numerics;
using ImGuiNET;

namespace Despada.ImGui.UserInterface;

public static partial class Widgets
{
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
}