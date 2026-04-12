using System.Numerics;
using Despada.ImGui;
using Despada.ImGui.UserInterface.Tabs;
using ImGuiNET;

namespace Despada.ImGui.UserInterface;

public static class MainMenu
{
    private static readonly string[] TabIcons  = ["\uF06E", "\uF0E7", "\uF05B", "\uF0AD"];
    private static readonly string[] TabLabels = ["Visuals", "Movement", "Combat", "Misc"];

    private const string SettingsIcon  = "\uF013";
    private const string SettingsLabel = "Settings";
    private const int    SettingsIndex = 4;

    private static readonly string[][] SubTabs =
    [
        ["Players", "Items", "World"],
        ["Speed", "Teleport"],
        ["Targeting", "Automation"],
        ["Utilities", "Info"],
        ["General", "Appearance", "Debug"],
    ];

    private static int _activeTab;
    private static int _activeSubTab;

    private const float WinW     = 1100f;
    private const float WinH     = 740f;
    private const float SidebarW = 210f;
    private const float TabH     = 56f;
    private const float TopBarH  = 52f;
    private const float IconScale = 1.5f;
    private const float ColumnGap = 12f;
    
    private static readonly uint SidebarBg = Theme.ToU32(Theme.Hex(0x0A0A0E));
    private static readonly uint SepLine   = Theme.ToU32(Theme.HexA(0xFFFFFF, 0.08f));

    private static bool _firstFrame = true;

    public static void Draw()
    {
        if (_firstFrame)
        {
            var d = ImGuiNET.ImGui.GetIO().DisplaySize;
            ImGuiNET.ImGui.SetNextWindowPos(new Vector2((d.X - WinW) * 0.5f, (d.Y - WinH) * 0.5f));
            _firstFrame = false;
        }

        ImGuiNET.ImGui.SetNextWindowSize(new Vector2(WinW, WinH));

        ImGuiNET.ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.Zero);
        ImGuiNET.ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 10f);
        ImGuiNET.ImGui.PushStyleColor(ImGuiCol.WindowBg, Theme.Hex(0x08080C));
        ImGuiNET.ImGui.PushStyleColor(ImGuiCol.Border, Theme.HexA(0xFFFFFF, 0.06f));

        var flags = ImGuiWindowFlags.NoTitleBar
                  | ImGuiWindowFlags.NoCollapse
                  | ImGuiWindowFlags.NoResize
                  | ImGuiWindowFlags.NoScrollbar
                  | ImGuiWindowFlags.NoScrollWithMouse;

        if (!ImGuiNET.ImGui.Begin("##despada_main", flags))
        {
            ImGuiNET.ImGui.End();
            ImGuiNET.ImGui.PopStyleColor(2);
            ImGuiNET.ImGui.PopStyleVar(2);
            return;
        }

        ImGuiNET.ImGui.PopStyleColor(2);
        ImGuiNET.ImGui.PopStyleVar(2);

        var winPos = ImGuiNET.ImGui.GetWindowPos();
        var dl     = ImGuiNET.ImGui.GetWindowDrawList();
        var font   = ImGuiNET.ImGui.GetFont();

        Widgets.ResetFrame();

        DrawSidebar(dl, winPos, font);
        DrawTopBar(dl, winPos, font);
        DrawContent();

        ImGuiNET.ImGui.End();
    }

    private static void DrawSidebar(ImDrawListPtr dl, Vector2 wp, ImFontPtr font)
    {
        var max = new Vector2(wp.X + SidebarW, wp.Y + WinH);

        dl.AddRectFilled(wp, max, SidebarBg, 10f,
            ImDrawFlags.RoundCornersTopLeft | ImDrawFlags.RoundCornersBottomLeft);

        dl.AddLine(new Vector2(max.X, wp.Y + 6f), new Vector2(max.X, max.Y - 6f), SepLine);

        var bf = font.FontSize * 1.6f;
        var brandText = "DESPADA";
        var bs = font.CalcTextSizeA(bf, float.MaxValue, 0f, brandText);
        Widgets.DrawGradientText(dl, font, bf,
            new Vector2(wp.X + (SidebarW - bs.X) * 0.5f - 2f, wp.Y + (TopBarH - bs.Y) * 0.5f),
            brandText, Theme.Violet, Theme.Cyan, Theme.Magenta, 0.18f);

        dl.AddLine(
            new Vector2(wp.X + 16f, wp.Y + TopBarH),
            new Vector2(max.X - 16f, wp.Y + TopBarH),
            SepLine);

        var ifs = font.FontSize * IconScale;
        var ty  = wp.Y + TopBarH + 12f;

        for (int i = 0; i < TabIcons.Length; i++)
            SidebarTab(dl, font, ifs, wp, ty + TabH * i, TabIcons[i], TabLabels[i], i);

        SidebarTab(dl, font, ifs, wp, max.Y - TabH, SettingsIcon, SettingsLabel, SettingsIndex);
    }

    private static readonly uint BarTop    = Theme.ToU32(Theme.Violet);
    private static readonly uint BarMid    = Theme.ToU32(Theme.Cyan);
    private static readonly uint BarBottom = Theme.ToU32(Theme.Magenta);

    private static void SidebarTab(ImDrawListPtr dl, ImFontPtr font, float fs,
        Vector2 wp, float y, string icon, string label, int idx)
    {
        var tMin = new Vector2(wp.X, y);
        var tMax = new Vector2(wp.X + SidebarW - 1f, y + TabH);

        bool active  = idx == _activeTab;
        bool hovered = ImGuiNET.ImGui.IsMouseHoveringRect(tMin, tMax);
        bool clicked = hovered && ImGuiNET.ImGui.IsMouseClicked(ImGuiMouseButton.Left);

        if (active)       dl.AddRectFilled(tMin, tMax, Theme.ToU32(Theme.CyanMuted));
        else if (hovered) dl.AddRectFilled(tMin, tMax, Theme.ToU32(Theme.BgHover));

        if (active)
        {
            var barL = new Vector2(tMin.X, tMin.Y + 8f);
            var barLMax = new Vector2(tMin.X + 3f, tMax.Y - 8f);
            var barMidY = (barL.Y + barLMax.Y) * 0.5f;
            dl.AddRectFilledMultiColor(barL, new Vector2(barLMax.X, barMidY),
                BarTop, BarTop, BarMid, BarMid);
            dl.AddRectFilledMultiColor(new Vector2(barL.X, barMidY), barLMax,
                BarMid, BarMid, BarBottom, BarBottom);

            var barR = new Vector2(tMax.X - 2f, tMin.Y + 8f);
            var barRMax = new Vector2(tMax.X, tMax.Y - 8f);
            var barRMidY = (barR.Y + barRMax.Y) * 0.5f;
            dl.AddRectFilledMultiColor(barR, new Vector2(barRMax.X, barRMidY),
                BarTop, BarTop, BarMid, BarMid);
            dl.AddRectFilledMultiColor(new Vector2(barR.X, barRMidY), barRMax,
                BarMid, BarMid, BarBottom, BarBottom);
        }

        var ic = active ? Theme.Cyan : hovered ? Theme.TextPrimary : Theme.TextSecondary;
        var isz = font.CalcTextSizeA(fs, float.MaxValue, 0f, icon);
        dl.AddText(font, fs, new Vector2(tMin.X + 24f, tMin.Y + (TabH - isz.Y) * 0.5f), Theme.ToU32(ic), icon);

        var lc = active || hovered ? Theme.TextPrimary : Theme.TextSecondary;
        var lsz = ImGuiNET.ImGui.CalcTextSize(label);
        dl.AddText(new Vector2(tMin.X + 24f + isz.X + 14f, tMin.Y + (TabH - lsz.Y) * 0.5f), Theme.ToU32(lc), label);

        if (clicked && idx != _activeTab) { _activeTab = idx; _activeSubTab = 0; }
    }

    private static void DrawTopBar(ImDrawListPtr dl, Vector2 wp, ImFontPtr font)
    {
        var bMin = new Vector2(wp.X + SidebarW, wp.Y);
        var bMax = new Vector2(wp.X + WinW, wp.Y + TopBarH);

        dl.AddLine(new Vector2(bMin.X + 8f, bMax.Y), new Vector2(bMax.X - 8f, bMax.Y), SepLine);

        var subs = SubTabs[_activeTab];
        var sx   = bMin.X + 24f;
        var subFs = font.FontSize * 1.05f;

        for (int i = 0; i < subs.Length; i++)
        {
            var tsz = font.CalcTextSizeA(subFs, float.MaxValue, 0f, subs[i]);
            var tw  = tsz.X + 40f;
            var tMin = new Vector2(sx, bMin.Y);
            var tMax = new Vector2(sx + tw, bMax.Y);

            bool active  = i == _activeSubTab;
            bool hovered = ImGuiNET.ImGui.IsMouseHoveringRect(tMin, tMax);

            var c = active || hovered ? Theme.TextPrimary : Theme.TextSecondary;
            dl.AddText(font, subFs,
                new Vector2(tMin.X + (tw - tsz.X) * 0.5f, tMin.Y + (TopBarH - tsz.Y) * 0.5f),
                Theme.ToU32(c), subs[i]);

            if (active)
            {
                var lw = tsz.X * 0.6f;
                var lx = tMin.X + (tw - lw) * 0.5f;
                dl.AddRectFilledMultiColor(
                    new Vector2(lx, bMax.Y - 3f), new Vector2(lx + lw, bMax.Y),
                    BarTop, BarBottom, BarBottom, BarTop);
            }

            if (hovered && ImGuiNET.ImGui.IsMouseClicked(ImGuiMouseButton.Left))
                _activeSubTab = i;

            sx += tw;
        }
    }

    private static void DrawContent()
    {
        var cx = SidebarW + 1f;
        var cy = TopBarH + 1f;
        var cw = WinW - SidebarW - 1f;
        var ch = WinH - TopBarH - 1f;
        var m  = 12f;

        ImGuiNET.ImGui.SetCursorPos(new Vector2(cx + 16f, cy + m));

        ImGuiNET.ImGui.PushStyleColor(ImGuiCol.ScrollbarBg, Theme.HexA(0x000000, 0.05f));
        ImGuiNET.ImGui.BeginChild("##content",
            new Vector2(cw - 42f, ch - m * 2f),
            ImGuiChildFlags.None, ImGuiWindowFlags.NoBackground);

        var colW = (ImGuiNET.ImGui.GetContentRegionAvail().X - ColumnGap) * 0.5f;

        ImGuiNET.ImGui.BeginChild("##col_left", new Vector2(colW, 0f),
            ImGuiChildFlags.AutoResizeY,
            ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoScrollbar);
        RouteTab(0);
        ImGuiNET.ImGui.EndChild();

        ImGuiNET.ImGui.SameLine(0f, ColumnGap);

        ImGuiNET.ImGui.BeginChild("##col_right", new Vector2(colW, 0f),
            ImGuiChildFlags.AutoResizeY,
            ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoScrollbar);
        RouteTab(1);
        ImGuiNET.ImGui.EndChild();

        ImGuiNET.ImGui.EndChild();
        ImGuiNET.ImGui.PopStyleColor();
    }

    private static void RouteTab(int col)
    {
        switch (_activeTab)
        {
            case 0: VisualsTab.Draw(_activeSubTab, col);  break;
            case 1: MovementTab.Draw(_activeSubTab, col);  break;
            case 2: CombatTab.Draw(_activeSubTab, col);    break;
            case 3: MiscTab.Draw(_activeSubTab, col);      break;
            case 4: SettingsTab.Draw(_activeSubTab, col);  break;
        }
    }
}