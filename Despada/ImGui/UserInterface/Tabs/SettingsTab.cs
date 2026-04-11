using Despada.ImGui;
using ImGuiNET;

namespace Despada.ImGui.UserInterface.Tabs;

public static class SettingsTab
{
    private static bool _keybindsEnabled = true;
    private static bool _configEnabled = true;
    private static bool _themeEnabled = true;
    private static bool _debugEnabled = true;

    public static void Draw(int subTab, int col)
    {
        switch (subTab)
        {
            case 0:
                if (col == 0)
                {
                    if (Widgets.BeginFeatureSection("\uF11C", "Keybinds", "Configure key bindings", ref _keybindsEnabled))
                    {
                        ImGuiNET.ImGui.TextColored(Theme.TextDisabled, "Coming soon...");
                    }
                    Widgets.EndFeatureSection();
                }
                else
                {
                    if (Widgets.BeginFeatureSection("\uF0C7", "Config", "Save and load configs", ref _configEnabled))
                    {
                        ImGuiNET.ImGui.TextColored(Theme.TextDisabled, "Save / Load...");
                    }
                    Widgets.EndFeatureSection();
                }
                break;

            case 1:
                if (col == 0)
                {
                    if (Widgets.BeginFeatureSection("\uF53F", "Theme", "Customize colors and fonts", ref _themeEnabled))
                    {
                        ImGuiNET.ImGui.TextColored(Theme.TextDisabled, "Colors, fonts...");
                    }
                    Widgets.EndFeatureSection();
                }
                break;

            case 2:
                if (col == 0)
                {
                    if (Widgets.BeginFeatureSection("\uF188", "Debug Tools", "Development and testing", ref _debugEnabled))
                    {
                        if (ImGuiNET.ImGui.Button("Test Toast"))
                            ImGuiRenderer.ShowToast("Despada", "Settings saved!", 3f);
                    }
                    Widgets.EndFeatureSection();
                }
                break;
        }
    }
}