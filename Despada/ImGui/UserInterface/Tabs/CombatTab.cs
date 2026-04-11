using Despada.ImGui;
using ImGuiNET;

namespace Despada.ImGui.UserInterface.Tabs;

public static class CombatTab
{
    private static bool _targetingEnabled;
    private static bool _automationEnabled;

    public static void Draw(int subTab, int col)
    {
        switch (subTab)
        {
            case 0:
                if (col == 0)
                {
                    if (Widgets.BeginFeatureSection("\uF05B", "Targeting", "Auto-targeting settings", ref _targetingEnabled))
                    {
                        ImGuiNET.ImGui.TextColored(Theme.TextDisabled, "Coming soon...");
                    }
                    Widgets.EndFeatureSection();
                }
                break;

            case 1:
                if (col == 0)
                {
                    if (Widgets.BeginFeatureSection("\uF085", "Auto Actions", "Automated combat actions", ref _automationEnabled))
                    {
                        ImGuiNET.ImGui.TextColored(Theme.TextDisabled, "Coming soon...");
                    }
                    Widgets.EndFeatureSection();
                }
                break;
        }
    }
}