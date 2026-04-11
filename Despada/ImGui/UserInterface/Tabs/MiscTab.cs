using Despada.ImGui;
using ImGuiNET;

namespace Despada.ImGui.UserInterface.Tabs;

public static class MiscTab
{
    private static bool _utilitiesEnabled;
    private static bool _gameInfoEnabled;

    public static void Draw(int subTab, int col)
    {
        switch (subTab)
        {
            case 0:
                if (col == 0)
                {
                    if (Widgets.BeginFeatureSection("\uF0AD", "Utilities", "Misc tools and helpers", ref _utilitiesEnabled))
                    {
                        ImGuiNET.ImGui.TextColored(Theme.TextDisabled, "Coming soon...");
                    }
                    Widgets.EndFeatureSection();
                }
                break;

            case 1:
                if (col == 0)
                {
                    if (Widgets.BeginFeatureSection("\uF05A", "Game Info", "Display game information", ref _gameInfoEnabled))
                    {
                        ImGuiNET.ImGui.TextColored(Theme.TextDisabled, "Coming soon...");
                    }
                    Widgets.EndFeatureSection();
                }
                break;
        }
    }
}