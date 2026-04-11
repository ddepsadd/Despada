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
    
    private const float WinW     = 1000f;
    private const float WinH     = 700f;
    private const float SidebarW = 170f;
    private const float TabH     = 52f;
    private const float TopBarH  = 48f;
    private const float IconScale = 1.3f;
    private const float ColumnGap = 10f;
    
    private static readonly uint SidebarBg   = Theme.ToU32(Theme.Hex(0x070712));
    private static readonly uint SepLine     = Theme.ToU32(Theme.HexA(0x9B30FF, 0.10f));
    private static readonly uint GlowPrimary = Theme.ToU32(Theme.HexA(0x6B2FD6, 0.08f));
    private static readonly uint GlowAccent  = Theme.ToU32(Theme.HexA(0xFF3090, 0.06f));
    private static readonly uint GlowNone    = Theme.ToU32(Theme.HexA(0x000000, 0.00f));

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
        ImGuiNET.ImGui.PushStyleColor(ImGuiCol.WindowBg, Theme.Hex(0x060610));
        ImGuiNET.ImGui.PushStyleColor(ImGuiCol.Border, Theme.HexA(0x9B30FF, 0.08f));

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

        DrawGlow(dl, winPos);
        DrawSidebar(dl, winPos, font);
        DrawTopBar(dl, winPos, font);
        DrawContent();

        ImGuiNET.ImGui.End();
    }

    private static void DrawGlow(ImDrawListPtr dl, Vector2 wp)
    {
        dl.AddRectFilledMultiColor(wp,
            new Vector2(wp.X + WinW, wp.Y + WinH),
            GlowPrimary, GlowNone, GlowAccent, GlowNone);
    }

    private static void DrawSidebar(ImDrawListPtr dl, Vector2 wp, ImFontPtr font)
    {
        var max = new Vector2(wp.X + SidebarW, wp.Y + WinH);

        dl.AddRectFilled(wp, max, SidebarBg, 10f,
            ImDrawFlags.RoundCornersTopLeft | ImDrawFlags.RoundCornersBottomLeft);

        dl.AddLine(new Vector2(max.X, wp.Y + 6f), new Vector2(max.X, max.Y - 6f), SepLine);

        var bf = font.FontSize * 1.4f;
        var bs = font.CalcTextSizeA(bf, float.MaxValue, 0f, "Despada");
        dl.AddText(font, bf,
            new Vector2(wp.X + (SidebarW - bs.X) * 0.5f, wp.Y + (TopBarH - bs.Y) * 0.5f),
            Theme.ToU32(Theme.Cyan), "Despada");

        dl.AddLine(
            new Vector2(wp.X + 12f, wp.Y + TopBarH),
            new Vector2(max.X - 12f, wp.Y + TopBarH),
            SepLine);

        var ifs = font.FontSize * IconScale;
        var ty  = wp.Y + TopBarH + 8f;

        for (int i = 0; i < TabIcons.Length; i++)
            SidebarTab(dl, font, ifs, wp, ty + TabH * i, TabIcons[i], TabLabels[i], i);

        SidebarTab(dl, font, ifs, wp, max.Y - TabH, SettingsIcon, SettingsLabel, SettingsIndex);
    }

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
            dl.AddRectFilled(new Vector2(tMin.X, tMin.Y + 8f),
                new Vector2(tMin.X + 3f, tMax.Y - 8f), Theme.ToU32(Theme.Cyan), 2f);

        var ic = active ? Theme.Cyan : hovered ? Theme.TextPrimary : Theme.TextSecondary;
        var isz = font.CalcTextSizeA(fs, float.MaxValue, 0f, icon);
        dl.AddText(font, fs, new Vector2(tMin.X + 18f, tMin.Y + (TabH - isz.Y) * 0.5f), Theme.ToU32(ic), icon);

        var lc = active || hovered ? Theme.TextPrimary : Theme.TextSecondary;
        var lsz = ImGuiNET.ImGui.CalcTextSize(label);
        dl.AddText(new Vector2(tMin.X + 18f + isz.X + 12f, tMin.Y + (TabH - lsz.Y) * 0.5f), Theme.ToU32(lc), label);

        if (clicked && idx != _activeTab) { _activeTab = idx; _activeSubTab = 0; }
    }

    private static void DrawTopBar(ImDrawListPtr dl, Vector2 wp, ImFontPtr font)
    {
        var bMin = new Vector2(wp.X + SidebarW, wp.Y);
        var bMax = new Vector2(wp.X + WinW, wp.Y + TopBarH);

        dl.AddLine(new Vector2(bMin.X + 8f, bMax.Y), new Vector2(bMax.X - 8f, bMax.Y), SepLine);

        var subs = SubTabs[_activeTab];
        var sx   = bMin.X + 20f;

        for (int i = 0; i < subs.Length; i++)
        {
            var tsz = ImGuiNET.ImGui.CalcTextSize(subs[i]);
            var tw  = tsz.X + 32f;
            var tMin = new Vector2(sx, bMin.Y);
            var tMax = new Vector2(sx + tw, bMax.Y);

            bool active  = i == _activeSubTab;
            bool hovered = ImGuiNET.ImGui.IsMouseHoveringRect(tMin, tMax);

            var c = active || hovered ? Theme.TextPrimary : Theme.TextSecondary;
            dl.AddText(
                new Vector2(tMin.X + (tw - tsz.X) * 0.5f, tMin.Y + (TopBarH - tsz.Y) * 0.5f),
                Theme.ToU32(c), subs[i]);

            if (active)
            {
                var lw = tsz.X * 0.6f;
                var lx = tMin.X + (tw - lw) * 0.5f;
                dl.AddRectFilled(new Vector2(lx, bMax.Y - 3f), new Vector2(lx + lw, bMax.Y),
                    Theme.ToU32(Theme.Cyan), 1.5f);
            }

            if (hovered && ImGuiNET.ImGui.IsMouseClicked(ImGuiMouseButton.Left))
                _activeSubTab = i;

            sx += tw;
        }
    }

    // ── Content ─────────────────────────────────────────────────────
    private static void DrawContent()
    {
        var cx = SidebarW + 1f;
        var cy = TopBarH + 1f;
        var cw = WinW - SidebarW - 1f;
        var ch = WinH - TopBarH - 1f;
        var m  = 16f;

        ImGuiNET.ImGui.SetCursorPos(new Vector2(cx + m, cy + m));

        ImGuiNET.ImGui.PushStyleColor(ImGuiCol.ScrollbarBg, Theme.HexA(0x000000, 0.05f));
        ImGuiNET.ImGui.BeginChild("##content",
            new Vector2(cw - m * 2f, ch - m * 2f),
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