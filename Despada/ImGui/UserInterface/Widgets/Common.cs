using System.Numerics;
using Despada.ImGui;
using ImGuiNET;

namespace Despada.ImGui.UserInterface;

public static partial class Widgets
{
    private static int _sectionId;
    private static readonly HashSet<string> _toggleUpdatedThisFrame = new();

    private static readonly Vector4 SectionBg      = Theme.Hex(0x141418);
    private static readonly uint    SectionBorder  = Theme.ToU32(Theme.HexA(0xFFFFFF, 0.08f));
    private static readonly uint    SeparatorColor = Theme.ToU32(Theme.HexA(0xFFFFFF, 0.08f));

    private const float SectionPad    = 12f;
    private const float SectionGap    = 14f;
    private const float SectionRound  = 10f;
    private const float ContentIndent = 16f;
    private const float HeaderPad     = 16f;
    private const float HeaderH       = 64f;

    public static void ResetFrame()
    {
        _sectionId = 0;
        _toggleUpdatedThisFrame.Clear();
    }
}