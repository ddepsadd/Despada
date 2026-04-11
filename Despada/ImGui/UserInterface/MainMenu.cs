using System.Numerics;
using Despada.ImGui;
using ImGuiNET;

namespace Despada.ImGui.UserInterface;

public static class MainMenu
{
    private static readonly string[] TabIcons =
    [
        "\uF06E",   // eye        Visuals
        "\uF0E7",   // bolt       Movement
        "\uF05B",   // crosshairs Combat
        "\uF0AD",   // wrench     Misc
    ];

    private static readonly string[] TabLabels =
    [
        "Visuals",
        "Movement",
        "Combat",
        "Misc",
    ];

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

    private const float WinW = 1000f;
    private const float WinH = 700f;

    private const float SidebarW   = 170f;
    private const float TabH       = 52f;
    private const float IconScale  = 1.3f;
    private const float TopBarH    = 48f;

    private const float ColumnGap  = 10f;
    private const float SectionPad = 12f;
    private const float SectionGap = 10f;

    private static readonly uint SidebarBg     = Theme.ToU32(Theme.Hex(0x070712));
    private static readonly uint SepLine       = Theme.ToU32(Theme.HexA(0x9B30FF, 0.10f));
    private static readonly Vector4 SectionBgColor = Theme.HexA(0x111120, 0.90f);

    private static readonly uint GlowPrimary = Theme.ToU32(Theme.HexA(0x6B2FD6, 0.08f));
    private static readonly uint GlowAccent  = Theme.ToU32(Theme.HexA(0xFF3090, 0.06f));
    private static readonly uint GlowNone    = Theme.ToU32(Theme.HexA(0x000000, 0.00f));

    private static bool _firstFrame = true;
    private static int  _sectionId;

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

        _sectionId = 0;

        DrawBackgroundGlow(dl, winPos);
        DrawSidebar(dl, winPos, font);
        DrawTopBar(dl, winPos, font);
        DrawContent();

        ImGuiNET.ImGui.End();
    }

    private static void DrawBackgroundGlow(ImDrawListPtr dl, Vector2 winPos)
    {
        var min = winPos;
        var max = new Vector2(winPos.X + WinW, winPos.Y + WinH);

        dl.AddRectFilledMultiColor(
            min, max,
            GlowPrimary,
            GlowNone,
            GlowAccent,
            GlowNone);
    }

    private static void DrawSidebar(ImDrawListPtr dl, Vector2 winPos, ImFontPtr font)
    {
        var min = winPos;
        var max = new Vector2(winPos.X + SidebarW, winPos.Y + WinH);

        dl.AddRectFilled(min, max, SidebarBg, 10f,
            ImDrawFlags.RoundCornersTopLeft | ImDrawFlags.RoundCornersBottomLeft);

        dl.AddLine(
            new Vector2(max.X, min.Y + 6f),
            new Vector2(max.X, max.Y - 6f),
            SepLine);

        var brandFontSize = font.FontSize * 1.4f;
        var brandText     = "Despada";
        var brandSize     = font.CalcTextSizeA(brandFontSize, float.MaxValue, 0f, brandText);
        var brandPos      = new Vector2(
            min.X + (SidebarW - brandSize.X) * 0.5f,
            min.Y + (TopBarH - brandSize.Y) * 0.5f);
        dl.AddText(font, brandFontSize, brandPos, Theme.ToU32(Theme.Cyan), brandText);

        dl.AddLine(
            new Vector2(min.X + 12f, min.Y + TopBarH),
            new Vector2(max.X - 12f, min.Y + TopBarH),
            SepLine);

        var iconFontSize = font.FontSize * IconScale;

        var tabStartY = min.Y + TopBarH + 8f;
        for (int i = 0; i < TabIcons.Length; i++)
            DrawSidebarTab(dl, font, iconFontSize, winPos, tabStartY + TabH * i, TabIcons[i], TabLabels[i], i);

        var settingsY = max.Y - TabH;
        DrawSidebarTab(dl, font, iconFontSize, winPos, settingsY, SettingsIcon, SettingsLabel, SettingsIndex);
    }

    private static void DrawSidebarTab(
        ImDrawListPtr dl, ImFontPtr font, float fontSize,
        Vector2 winPos, float y,
        string icon, string label, int index)
    {
        var tabMin = new Vector2(winPos.X, y);
        var tabMax = new Vector2(winPos.X + SidebarW - 1f, y + TabH);

        bool isActive  = index == _activeTab;
        bool isHovered = ImGuiNET.ImGui.IsMouseHoveringRect(tabMin, tabMax);
        bool isClicked = isHovered && ImGuiNET.ImGui.IsMouseClicked(ImGuiMouseButton.Left);

        if (isActive)
            dl.AddRectFilled(tabMin, tabMax, Theme.ToU32(Theme.CyanMuted));
        else if (isHovered)
            dl.AddRectFilled(tabMin, tabMax, Theme.ToU32(Theme.BgHover));

        if (isActive)
        {
            dl.AddRectFilled(
                new Vector2(tabMin.X, tabMin.Y + 8f),
                new Vector2(tabMin.X + 3f, tabMax.Y - 8f),
                Theme.ToU32(Theme.Cyan), 2f);
        }

        var iconColor = isActive ? Theme.Cyan : isHovered ? Theme.TextPrimary : Theme.TextSecondary;
        var iconSize  = font.CalcTextSizeA(fontSize, float.MaxValue, 0f, icon);
        var iconX     = tabMin.X + 18f;
        var iconY     = tabMin.Y + (TabH - iconSize.Y) * 0.5f;
        dl.AddText(font, fontSize, new Vector2(iconX, iconY), Theme.ToU32(iconColor), icon);

        var labelColor = isActive ? Theme.TextPrimary : isHovered ? Theme.TextPrimary : Theme.TextSecondary;
        var labelSize  = ImGuiNET.ImGui.CalcTextSize(label);
        var labelX     = iconX + iconSize.X + 12f;
        var labelY     = tabMin.Y + (TabH - labelSize.Y) * 0.5f;
        dl.AddText(new Vector2(labelX, labelY), Theme.ToU32(labelColor), label);

        if (isClicked && index != _activeTab)
        {
            _activeTab    = index;
            _activeSubTab = 0;
        }
    }

    private static void DrawTopBar(ImDrawListPtr dl, Vector2 winPos, ImFontPtr font)
    {
        var barMin = new Vector2(winPos.X + SidebarW, winPos.Y);
        var barMax = new Vector2(winPos.X + WinW, winPos.Y + TopBarH);

        dl.AddLine(
            new Vector2(barMin.X + 8f, barMax.Y),
            new Vector2(barMax.X - 8f, barMax.Y),
            SepLine);

        var subs = SubTabs[_activeTab];
        var subX = barMin.X + 20f;

        for (int i = 0; i < subs.Length; i++)
        {
            var text     = subs[i];
            var textSize = ImGuiNET.ImGui.CalcTextSize(text);
            var padX     = 16f;
            var tabW     = textSize.X + padX * 2f;

            var tabMin = new Vector2(subX, barMin.Y);
            var tabMax = new Vector2(subX + tabW, barMax.Y);

            bool isActive  = i == _activeSubTab;
            bool isHovered = ImGuiNET.ImGui.IsMouseHoveringRect(tabMin, tabMax);
            bool isClicked = isHovered && ImGuiNET.ImGui.IsMouseClicked(ImGuiMouseButton.Left);

            var color   = isActive ? Theme.TextPrimary : isHovered ? Theme.TextPrimary : Theme.TextSecondary;
            var textPos = new Vector2(
                tabMin.X + (tabW - textSize.X) * 0.5f,
                tabMin.Y + (TopBarH - textSize.Y) * 0.5f);
            dl.AddText(textPos, Theme.ToU32(color), text);

            if (isActive)
            {
                var lineW = textSize.X * 0.6f;
                var lineX = tabMin.X + (tabW - lineW) * 0.5f;
                dl.AddRectFilled(
                    new Vector2(lineX, barMax.Y - 3f),
                    new Vector2(lineX + lineW, barMax.Y),
                    Theme.ToU32(Theme.Cyan), 1.5f);
            }

            if (isClicked)
                _activeSubTab = i;

            subX += tabW;
        }
    }

    private static void DrawContent()
    {
        var contentX = SidebarW + 1f;
        var contentY = TopBarH + 1f;
        var contentW = WinW - SidebarW - 1f;
        var contentH = WinH - TopBarH - 1f;
        var margin   = 16f;

        ImGuiNET.ImGui.SetCursorPos(new Vector2(contentX + margin, contentY + margin));

        ImGuiNET.ImGui.PushStyleColor(ImGuiCol.ScrollbarBg, Theme.HexA(0x000000, 0.05f));
        ImGuiNET.ImGui.BeginChild("##content",
            new Vector2(contentW - margin * 2f, contentH - margin * 2f),
            ImGuiChildFlags.None,
            ImGuiWindowFlags.NoBackground);

        var colAvailW = ImGuiNET.ImGui.GetContentRegionAvail().X;
        var colW      = (colAvailW - ColumnGap) * 0.5f;

        ImGuiNET.ImGui.BeginChild("##col_left", new Vector2(colW, 0f),
            ImGuiChildFlags.AutoResizeY,
            ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoScrollbar);
        DrawSubTabContent(0);
        ImGuiNET.ImGui.EndChild();

        ImGuiNET.ImGui.SameLine(0f, ColumnGap);

        ImGuiNET.ImGui.BeginChild("##col_right", new Vector2(colW, 0f),
            ImGuiChildFlags.AutoResizeY,
            ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoScrollbar);
        DrawSubTabContent(1);
        ImGuiNET.ImGui.EndChild();

        ImGuiNET.ImGui.EndChild();
        ImGuiNET.ImGui.PopStyleColor();
    }

    private static void BeginSection(string title)
    {
        _sectionId++;
        var availW = ImGuiNET.ImGui.GetContentRegionAvail().X;

        ImGuiNET.ImGui.PushStyleColor(ImGuiCol.ChildBg, SectionBgColor);
        ImGuiNET.ImGui.PushStyleVar(ImGuiStyleVar.ChildRounding, 6f);
        ImGuiNET.ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(SectionPad, SectionPad));

        ImGuiNET.ImGui.BeginChild($"##section_{_sectionId}", new Vector2(availW, 0f),
            ImGuiChildFlags.AutoResizeY | ImGuiChildFlags.AlwaysAutoResize,
            ImGuiWindowFlags.NoScrollbar);

        ImGuiNET.ImGui.TextColored(Theme.TextSecondary, title);
        ImGuiNET.ImGui.Spacing();
    }

    private static void EndSection()
    {
        ImGuiNET.ImGui.EndChild();
        ImGuiNET.ImGui.PopStyleVar(2);
        ImGuiNET.ImGui.PopStyleColor();
        ImGuiNET.ImGui.Dummy(new Vector2(0f, SectionGap));
    }

    private static void DrawSubTabContent(int col)
    {
        switch (_activeTab)
        {
            case 0: DrawVisuals(col);  break;
            case 1: DrawMovement(col); break;
            case 2: DrawCombat(col);   break;
            case 3: DrawMisc(col);     break;
            case 4: DrawSettings(col); break;
        }
    }

    private static bool _espPlayers, _espItems, _espHealth, _espNames = true;
    private static bool _espBox, _espTracers;
    private static float _espDistance = 50f;
    private static int _espColorMode;
    private static bool _itemWeapons, _itemMedical, _itemValuables;

    private static void DrawVisuals(int col)
    {
        switch (_activeSubTab)
        {
            case 0:
                if (col == 0)
                {
                    BeginSection("Player ESP");
                    ImGuiNET.ImGui.Checkbox("Enabled", ref _espPlayers);
                    ImGuiNET.ImGui.Checkbox("Health Bars", ref _espHealth);
                    ImGuiNET.ImGui.Checkbox("Names", ref _espNames);
                    ImGuiNET.ImGui.PushItemWidth(-1f);
                    ImGuiNET.ImGui.SliderFloat("##dist", ref _espDistance, 0f, 100f, "Distance: %.0f");
                    ImGuiNET.ImGui.PopItemWidth();
                    EndSection();
                }
                else
                {
                    BeginSection("Drawing");
                    ImGuiNET.ImGui.Checkbox("Box ESP", ref _espBox);
                    ImGuiNET.ImGui.Checkbox("Tracers", ref _espTracers);
                    ImGuiNET.ImGui.PushItemWidth(-1f);
                    ImGuiNET.ImGui.Combo("##colormode", ref _espColorMode, "Team\0Role\0Health\0");
                    ImGuiNET.ImGui.PopItemWidth();
                    EndSection();
                }
                break;

            case 1:
                if (col == 0)
                {
                    BeginSection("Item Filters");
                    ImGuiNET.ImGui.Checkbox("Weapons", ref _itemWeapons);
                    ImGuiNET.ImGui.Checkbox("Medical", ref _itemMedical);
                    ImGuiNET.ImGui.Checkbox("Valuables", ref _itemValuables);
                    EndSection();
                }
                else
                {
                    BeginSection("Item Rendering");
                    ImGuiNET.ImGui.TextColored(Theme.TextDisabled, "Coming soon...");
                    EndSection();
                }
                break;

            case 2:
                if (col == 0)
                {
                    BeginSection("World ESP");
                    ImGuiNET.ImGui.TextColored(Theme.TextDisabled, "Coming soon...");
                    EndSection();
                }
                break;
        }
    }
    private static void DrawMovement(int col)
    {
        switch (_activeSubTab)
        {
            case 0:
                if (col == 0)
                {
                    BeginSection("Speed Hack");
                    ImGuiNET.ImGui.TextColored(Theme.TextDisabled, "Coming soon...");
                    EndSection();
                }
                break;

            case 1:
                if (col == 0)
                {
                    BeginSection("Teleport");
                    ImGuiNET.ImGui.TextColored(Theme.TextDisabled, "Coming soon...");
                    EndSection();
                }
                break;
        }
    }

    private static void DrawCombat(int col)
    {
        switch (_activeSubTab)
        {
            case 0:
                if (col == 0)
                {
                    BeginSection("Targeting");
                    ImGuiNET.ImGui.TextColored(Theme.TextDisabled, "Coming soon...");
                    EndSection();
                }
                break;

            case 1:
                if (col == 0)
                {
                    BeginSection("Auto Actions");
                    ImGuiNET.ImGui.TextColored(Theme.TextDisabled, "Coming soon...");
                    EndSection();
                }
                break;
        }
    }

    private static void DrawMisc(int col)
    {
        switch (_activeSubTab)
        {
            case 0:
                if (col == 0)
                {
                    BeginSection("Utilities");
                    ImGuiNET.ImGui.TextColored(Theme.TextDisabled, "Coming soon...");
                    EndSection();
                }
                break;

            case 1:
                if (col == 0)
                {
                    BeginSection("Game Info");
                    ImGuiNET.ImGui.TextColored(Theme.TextDisabled, "Coming soon...");
                    EndSection();
                }
                break;
        }
    }

    private static void DrawSettings(int col)
    {
        switch (_activeSubTab)
        {
            case 0:
                if (col == 0)
                {
                    BeginSection("Keybinds");
                    ImGuiNET.ImGui.TextColored(Theme.TextDisabled, "Coming soon...");
                    EndSection();
                }
                else
                {
                    BeginSection("Config");
                    ImGuiNET.ImGui.TextColored(Theme.TextDisabled, "Save / Load...");
                    EndSection();
                }
                break;

            case 1:
                if (col == 0)
                {
                    BeginSection("Theme");
                    ImGuiNET.ImGui.TextColored(Theme.TextDisabled, "Colors, fonts...");
                    EndSection();
                }
                break;

            case 2:
                if (col == 0)
                {
                    BeginSection("Debug Tools");
                    if (ImGuiNET.ImGui.Button("Test Toast"))
                        ImGuiRenderer.ShowToast("Despada", "Settings saved!", 3f);
                    EndSection();
                }
                break;
        }
    }
}