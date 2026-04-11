using Despada.ImGui;
using ImGuiNET;

namespace Despada.ImGui.UserInterface.Tabs;

public static class VisualsTab
{
    private static bool  _espEnabled = true;
    private static bool  _espHealth;
    private static bool  _espNames = true;
    private static float _espDistance = 50f;

    private static bool _drawingEnabled;
    private static bool _espBox;
    private static bool _espTracers;
    private static int  _espColorMode;

    private static bool _itemsEnabled;
    private static bool _itemWeapons;
    private static bool _itemMedical;
    private static bool _itemValuables;

    private static bool _itemRenderEnabled;
    private static bool _worldEspEnabled;

    public static void Draw(int subTab, int col)
    {
        switch (subTab)
        {
            case 0: DrawPlayers(col); break;
            case 1: DrawItems(col);   break;
            case 2: DrawWorld(col);   break;
        }
    }

    private static void DrawPlayers(int col)
    {
        if (col == 0)
        {
            if (Widgets.BeginFeatureSection("\uF06E", "Player ESP", "Show player locations", ref _espEnabled))
            {
                Widgets.ToggleRow("Health Bars", ref _espHealth);
                Widgets.ToggleRow("Names", ref _espNames);
                Widgets.SliderFloat("##dist", ref _espDistance, 0f, 100f, "Distance: %.0f");
            }
            Widgets.EndFeatureSection();
        }
        else
        {
            if (Widgets.BeginFeatureSection("\uF1FC", "Drawing", "ESP rendering options", ref _drawingEnabled))
            {
                Widgets.ToggleRow("Box ESP", ref _espBox);
                Widgets.ToggleRow("Tracers", ref _espTracers);
                Widgets.Combo("##colormode", ref _espColorMode, "Team\0Role\0Health\0");
            }
            Widgets.EndFeatureSection();
        }
    }

    private static void DrawItems(int col)
    {
        if (col == 0)
        {
            if (Widgets.BeginFeatureSection("\uF187", "Item Filters", "Choose which items to show", ref _itemsEnabled))
            {
                Widgets.ToggleRow("Weapons", ref _itemWeapons);
                Widgets.ToggleRow("Medical", ref _itemMedical);
                Widgets.ToggleRow("Valuables", ref _itemValuables);
            }
            Widgets.EndFeatureSection();
        }
        else
        {
            if (Widgets.BeginFeatureSection("\uF1FC", "Item Rendering", "How items are drawn", ref _itemRenderEnabled))
            {
                ImGuiNET.ImGui.TextColored(Theme.TextDisabled, "Coming soon...");
            }
            Widgets.EndFeatureSection();
        }
    }

    private static void DrawWorld(int col)
    {
        if (col == 0)
        {
            if (Widgets.BeginFeatureSection("\uF0AC", "World ESP", "Show world objects", ref _worldEspEnabled))
            {
                ImGuiNET.ImGui.TextColored(Theme.TextDisabled, "Coming soon...");
            }
            Widgets.EndFeatureSection();
        }
    }
}