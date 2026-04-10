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

    private static int _activeTab;

    private const float WinW = 1200f;
    private const float WinH = 800f;

    private const float Pad         = 10f;
    private const float Gap         = 8f;
    private const float SidebarW    = 56f;
    private const float TabH        = 48f;
    private const float IslandRound = 10f;
    private const float IconScale   = 1.5f;

    private static readonly uint WinBg    = Theme.ToU32(Theme.HexA(0x000000, 0.50f));
    private static readonly uint IslandBg = Theme.ToU32(Theme.HexA(0x06060C, 0.75f));

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
        ImGuiNET.ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, IslandRound);
        ImGuiNET.ImGui.PushStyleColor(ImGuiCol.WindowBg, Theme.HexA(0x000000, 0.5f));
        ImGuiNET.ImGui.PushStyleColor(ImGuiCol.Border, Theme.HexA(0x000000, 0f));

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

        DrawSidebarIsland(dl, winPos, font);
        DrawContentIsland(dl, winPos);

        ImGuiNET.ImGui.End();
    }

    private static void DrawSidebarIsland(ImDrawListPtr dl, Vector2 winPos, ImFontPtr font)
    {
        var min = new Vector2(winPos.X + Pad, winPos.Y + Pad);
        var max = new Vector2(min.X + SidebarW, winPos.Y + WinH - Pad);

        dl.AddRectFilled(min, max, IslandBg, IslandRound);

        var iconFontSize = font.FontSize * IconScale;
        
        var tabStartY = min.Y + 8f;

        for (int i = 0; i < TabIcons.Length; i++)
        {
            DrawSidebarTab(dl, font, iconFontSize,
                min, max, tabStartY + TabH * i, TabIcons[i], i);
        }
        
        var settingsY = max.Y - TabH - 8f;
        DrawSidebarTab(dl, font, iconFontSize,
            min, max, settingsY, SettingsIcon, SettingsIndex);
    }

    private static void DrawSidebarTab(
        ImDrawListPtr dl, ImFontPtr font, float fontSize,
        Vector2 sideMin, Vector2 sideMax, float y,
        string icon, int index)
    {
        var tabMin = new Vector2(sideMin.X + 4f, y);
        var tabMax = new Vector2(sideMax.X - 4f, y + TabH);

        bool isActive  = index == _activeTab;
        bool isHovered = ImGuiNET.ImGui.IsMouseHoveringRect(tabMin, tabMax);
        bool isClicked = isHovered && ImGuiNET.ImGui.IsMouseClicked(ImGuiMouseButton.Left);

        if (isActive)
            dl.AddRectFilled(tabMin, tabMax, Theme.ToU32(Theme.CyanMuted), 6f);
        else if (isHovered)
            dl.AddRectFilled(tabMin, tabMax, Theme.ToU32(Theme.BgHover), 6f);

        if (isActive)
        {
            dl.AddRectFilled(
                new Vector2(sideMin.X, tabMin.Y + 10f),
                new Vector2(sideMin.X + 3f, tabMax.Y - 10f),
                Theme.ToU32(Theme.Cyan), 2f);
        }

        var color    = isActive ? Theme.Cyan : isHovered ? Theme.TextPrimary : Theme.TextSecondary;
        var iconSize = font.CalcTextSizeA(fontSize, float.MaxValue, 0f, icon);
        var iconPos  = new Vector2(
            tabMin.X + (tabMax.X - tabMin.X - iconSize.X) * 0.5f,
            tabMin.Y + (TabH - iconSize.Y) * 0.5f);

        dl.AddText(font, fontSize, iconPos, Theme.ToU32(color), icon);

        if (isClicked && index != _activeTab)
            _activeTab = index;
    }

    private static void DrawContentIsland(ImDrawListPtr dl, Vector2 winPos)
    {
        var min = new Vector2(winPos.X + Pad + SidebarW + Gap, winPos.Y + Pad);
        var max = new Vector2(winPos.X + WinW - Pad, winPos.Y + WinH - Pad);

        dl.AddRectFilled(min, max, IslandBg, IslandRound);

        var contentPad = 16f;
        ImGuiNET.ImGui.SetCursorScreenPos(new Vector2(min.X + contentPad, min.Y + contentPad));

        var childW = (max.X - min.X) - contentPad * 2f;
        var childH = (max.Y - min.Y) - contentPad * 2f;

        ImGuiNET.ImGui.BeginChild("##content", new Vector2(childW, childH), ImGuiChildFlags.None,
            ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoScrollbar);

        var label = _activeTab == SettingsIndex ? SettingsLabel : TabLabels[_activeTab];
        var icon  = _activeTab == SettingsIndex ? SettingsIcon  : TabIcons[_activeTab];

        ImGuiNET.ImGui.TextColored(Theme.Cyan, $"{icon}  {label}");
        ImGuiNET.ImGui.Spacing();
        ImGuiNET.ImGui.Separator();
        ImGuiNET.ImGui.Spacing();

        switch (_activeTab)
        {
            case 0: DrawVisuals();  break;
            case 1: DrawMovement(); break;
            case 2: DrawCombat();   break;
            case 3: DrawMisc();     break;
            case 4: DrawSettings(); break;
        }

        ImGuiNET.ImGui.EndChild();
    }

    private static void DrawVisuals()
    {
        ImGuiNET.ImGui.TextColored(Theme.TextSecondary, "ESP, wallhacks, player highlights...");
    }

    private static void DrawMovement()
    {
        ImGuiNET.ImGui.TextColored(Theme.TextSecondary, "Speed, noclip, teleport...");
    }

    private static void DrawCombat()
    {
        ImGuiNET.ImGui.TextColored(Theme.TextSecondary, "Aimbot, auto-attack, damage...");
    }

    private static void DrawMisc()
    {
        ImGuiNET.ImGui.TextColored(Theme.TextSecondary, "Utilities, automation...");
    }

    private static void DrawSettings()
    {
        ImGuiNET.ImGui.TextColored(Theme.TextSecondary, "Theme, keybinds, config...");
        ImGuiNET.ImGui.Spacing();
        if (ImGuiNET.ImGui.Button("Test Toast"))
            ImGuiRenderer.ShowToast("Despada", "Settings saved!", 3f);
    }
}