using System.Numerics;
using ImGuiNET;

namespace Despada.ImGui.UserInterface;

public static partial class Widgets
{
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
}