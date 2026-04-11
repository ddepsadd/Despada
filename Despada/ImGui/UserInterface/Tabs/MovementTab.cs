using Despada.ImGui;
using ImGuiNET;

namespace Despada.ImGui.UserInterface.Tabs;

public static class MovementTab
{
    private static bool _speedEnabled;
    private static bool _teleportEnabled;

    public static void Draw(int subTab, int col)
    {
        switch (subTab)
        {
            case 0:
                if (col == 0)
                {
                    if (Widgets.BeginFeatureSection("\uF0E7", "Speed Hack", "Modify movement speed", ref _speedEnabled))
                    {
                        ImGuiNET.ImGui.TextColored(Theme.TextDisabled, "Coming soon...");
                    }
                    Widgets.EndFeatureSection();
                }
                break;

            case 1:
                if (col == 0)
                {
                    if (Widgets.BeginFeatureSection("\uF124", "Teleport", "Teleport to locations", ref _teleportEnabled))
                    {
                        ImGuiNET.ImGui.TextColored(Theme.TextDisabled, "Coming soon...");
                    }
                    Widgets.EndFeatureSection();
                }
                break;
        }
    }
}