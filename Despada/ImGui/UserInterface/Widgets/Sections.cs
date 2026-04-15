using System.Numerics;
using ImGuiNET;

namespace Despada.ImGui.UserInterface;

public static partial class Widgets
{
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
        {
            DrawGradientText(dl, font, titleFs, new Vector2(textX, blockY),
                title, Theme.Violet, Theme.Cyan, Theme.Magenta);
        }
        else
        {
            dl.AddText(font, titleFs, new Vector2(textX, blockY),
                Theme.ToU32(Theme.TextSecondary), title);
        }

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
}