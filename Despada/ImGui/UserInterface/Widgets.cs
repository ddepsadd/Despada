using System.Numerics;
using Despada.ImGui;
using ImGuiNET;

namespace Despada.ImGui.UserInterface;

public static class Widgets
{
    private static int _sectionId;

    public static void ResetFrame() => _sectionId = 0;

    private static readonly Vector4 SectionBg = Theme.HexA(0x111120, 0.90f);
    private const float SectionPad   = 18f;
    private const float SectionGap   = 10f;
    private const float SectionRound = 6f;
    private const float ContentIndent = 8f;

    public static void BeginSection(string title)
    {
        _sectionId++;
        var availW = ImGuiNET.ImGui.GetContentRegionAvail().X;

        ImGuiNET.ImGui.PushStyleColor(ImGuiCol.ChildBg, SectionBg);
        ImGuiNET.ImGui.PushStyleVar(ImGuiStyleVar.ChildRounding, SectionRound);
        ImGuiNET.ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(SectionPad, SectionPad));

        ImGuiNET.ImGui.BeginChild($"##section_{_sectionId}", new Vector2(availW, 0f),
            ImGuiChildFlags.AutoResizeY | ImGuiChildFlags.AlwaysAutoResize,
            ImGuiWindowFlags.NoScrollbar);

        ImGuiNET.ImGui.TextColored(Theme.TextSecondary, title);
        ImGuiNET.ImGui.Spacing();
    }

    public static void EndSection()
    {
        ImGuiNET.ImGui.EndChild();
        ImGuiNET.ImGui.PopStyleVar(2);
        ImGuiNET.ImGui.PopStyleColor();
        ImGuiNET.ImGui.Dummy(new Vector2(0f, SectionGap));
    }

    public static bool BeginFeatureSection(string icon, string title, string description, ref bool enabled)
    {
        _sectionId++;
        var availW = ImGuiNET.ImGui.GetContentRegionAvail().X;

        ImGuiNET.ImGui.PushStyleColor(ImGuiCol.ChildBg, SectionBg);
        ImGuiNET.ImGui.PushStyleVar(ImGuiStyleVar.ChildRounding, SectionRound);
        ImGuiNET.ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(SectionPad, SectionPad));

        ImGuiNET.ImGui.BeginChild($"##feature_{_sectionId}", new Vector2(availW, 0f),
            ImGuiChildFlags.AutoResizeY | ImGuiChildFlags.AlwaysAutoResize,
            ImGuiWindowFlags.NoScrollbar);

        ImGuiNET.ImGui.Indent(ContentIndent);
        var contentW = ImGuiNET.ImGui.GetContentRegionAvail().X;
        var iconColor = enabled ? Theme.Cyan : Theme.TextSecondary;

        ImGuiNET.ImGui.SetWindowFontScale(1.8f);
        ImGuiNET.ImGui.TextColored(iconColor, icon);
        ImGuiNET.ImGui.SetWindowFontScale(1.0f);

        ImGuiNET.ImGui.SameLine(0f, 12f);

        ImGuiNET.ImGui.BeginGroup();
        ImGuiNET.ImGui.TextColored(enabled ? Theme.Cyan : Theme.TextSecondary, title);
        ImGuiNET.ImGui.TextColored(Theme.TextDisabled, description);
        ImGuiNET.ImGui.EndGroup();

        ImGuiNET.ImGui.SameLine(contentW - ToggleW);
        Toggle($"##ft_{_sectionId}", ref enabled);

        ImGuiNET.ImGui.Spacing();

        if (enabled)
        {
            DrawSectionDivider();
            ImGuiNET.ImGui.Spacing();
        }

        return enabled;
    }

    public static void EndFeatureSection()
    {
        ImGuiNET.ImGui.Unindent(ContentIndent);
        ImGuiNET.ImGui.EndChild();
        ImGuiNET.ImGui.PopStyleVar(2);
        ImGuiNET.ImGui.PopStyleColor();
        ImGuiNET.ImGui.Dummy(new Vector2(0f, SectionGap));
    }

    public static void DrawSectionDivider()
    {
        var dl    = ImGuiNET.ImGui.GetWindowDrawList();
        var pos   = ImGuiNET.ImGui.GetCursorScreenPos();
        var availW = ImGuiNET.ImGui.GetContentRegionAvail().X;

        dl.AddRectFilled(
            pos,
            new Vector2(pos.X + availW, pos.Y + 2f),
            Theme.ToU32(Theme.HexA(0x9B30FF, 0.20f)),
            1f);

        ImGuiNET.ImGui.Dummy(new Vector2(0f, 4f));
    }

    private const float ToggleW = 48f;
    private const float ToggleH = 26f;
    private const float KnobR   = 10f;
    private const float KnobPad = 3f;

    public static bool Toggle(string id, ref bool value)
    {
        var pos = ImGuiNET.ImGui.GetCursorScreenPos();

        var result = ImGuiNET.ImGui.InvisibleButton(id, new Vector2(ToggleW, ToggleH));
        if (result)
            value = !value;

        bool hovered = ImGuiNET.ImGui.IsItemHovered();
        var dl = ImGuiNET.ImGui.GetWindowDrawList();

        var min = pos;
        var max = new Vector2(pos.X + ToggleW, pos.Y + ToggleH);
        var radius = ToggleH * 0.5f;

        var trackColor = value
            ? (hovered ? Theme.Cyan : Theme.CyanDim)
            : (hovered ? Theme.BgHover : Theme.BgElevated);
        dl.AddRectFilled(min, max, Theme.ToU32(trackColor), radius);

        var knobX = value ? max.X - KnobR - KnobPad : min.X + KnobR + KnobPad;
        var knobColor = value ? Theme.Hex(0xFFFFFF) : Theme.HexA(0xFFFFFF, 0.5f);
        dl.AddCircleFilled(new Vector2(knobX, min.Y + ToggleH * 0.5f), KnobR, Theme.ToU32(knobColor));

        return result;
    }

    public static bool ToggleRow(string label, ref bool value)
    {
        var contentW = ImGuiNET.ImGui.GetContentRegionAvail().X;

        ImGuiNET.ImGui.AlignTextToFramePadding();
        ImGuiNET.ImGui.TextUnformatted(label);
        ImGuiNET.ImGui.SameLine(contentW - ToggleW);
        return Toggle($"##{label}_tgl", ref value);
    }

    public static bool SliderFloat(string label, ref float value, float min, float max, string format = "%.0f")
    {
        // Remove indent for full-width slider
        ImGuiNET.ImGui.Unindent(ContentIndent);
        ImGuiNET.ImGui.PushItemWidth(-1f);
        var result = ImGuiNET.ImGui.SliderFloat(label, ref value, min, max, format);
        ImGuiNET.ImGui.PopItemWidth();
        ImGuiNET.ImGui.Indent(ContentIndent);
        return result;
    }

    public static bool Combo(string label, ref int current, string items)
    {
        ImGuiNET.ImGui.Unindent(ContentIndent);
        ImGuiNET.ImGui.PushItemWidth(-1f);
        var result = ImGuiNET.ImGui.Combo(label, ref current, items);
        ImGuiNET.ImGui.PopItemWidth();
        ImGuiNET.ImGui.Indent(ContentIndent);
        return result;
    }
}