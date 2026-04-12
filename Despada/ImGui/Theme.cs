using System.Numerics;
using ImGuiNET;

namespace Despada.ImGui;

public static class Theme
{
    public static readonly Vector4 BgDeep       = Hex(0x08080C);
    public static readonly Vector4 BgBase       = Hex(0x0C0C10);
    public static readonly Vector4 BgSurface    = Hex(0x141418);
    public static readonly Vector4 BgElevated   = Hex(0x1A1A20);
    public static readonly Vector4 BgHover      = Hex(0x222228);

    public static readonly Vector4 Cyan         = Hex(0x9B30FF);
    public static readonly Vector4 CyanDim      = HexA(0x9B30FF, 0.70f);
    public static readonly Vector4 CyanMuted    = HexA(0x9B30FF, 0.15f);
    public static readonly Vector4 CyanGlow     = HexA(0x9B30FF, 0.35f);

    public static readonly Vector4 Magenta      = Hex(0xFF3090);
    public static readonly Vector4 MagentaDim   = HexA(0xFF3090, 0.70f);
    public static readonly Vector4 MagentaMuted = HexA(0xFF3090, 0.15f);

    public static readonly Vector4 Violet       = Hex(0x6B2FD6);
    public static readonly Vector4 VioletDim    = HexA(0x6B2FD6, 0.70f);
    public static readonly Vector4 VioletMuted  = HexA(0x6B2FD6, 0.15f);

    public static readonly Vector4 Green        = Hex(0x00E870);
    public static readonly Vector4 GreenDim     = HexA(0x00E870, 0.70f);
    public static readonly Vector4 GreenMuted   = HexA(0x00E870, 0.18f);

    public static readonly Vector4 Red          = Hex(0xFF1A4A);
    public static readonly Vector4 RedDim       = HexA(0xFF1A4A, 0.70f);
    public static readonly Vector4 RedMuted     = HexA(0xFF1A4A, 0.18f);

    public static readonly Vector4 Yellow       = Hex(0xFFD700);
    public static readonly Vector4 YellowMuted  = HexA(0xFFD700, 0.18f);

    public static readonly Vector4 TextPrimary  = Hex(0xE0E0E8);
    public static readonly Vector4 TextSecondary= Hex(0x888898);
    public static readonly Vector4 TextDisabled = Hex(0x505060);

    public static readonly Vector4 Border       = HexA(0xFFFFFF, 0.08f);
    public static readonly Vector4 BorderHover  = HexA(0xFFFFFF, 0.15f);
    public static readonly Vector4 BorderActive = HexA(0x9B30FF, 0.40f);

    public static readonly Vector4 ScrollBg     = HexA(0x000000, 0.10f);
    public static readonly Vector4 ScrollGrab   = HexA(0xFFFFFF, 0.10f);
    public static readonly Vector4 ScrollHover  = HexA(0xFFFFFF, 0.18f);
    public static readonly Vector4 ScrollActive = HexA(0xFFFFFF, 0.25f);

    public static void Apply()
    {
        var style  = ImGuiNET.ImGui.GetStyle();
        var colors = style.Colors;

        style.WindowRounding    = 8f;
        style.ChildRounding     = 6f;
        style.FrameRounding     = 5f;
        style.PopupRounding     = 6f;
        style.ScrollbarRounding = 4f;
        style.GrabRounding      = 4f;
        style.TabRounding       = 5f;

        style.WindowBorderSize  = 1f;
        style.ChildBorderSize   = 0f;
        style.FrameBorderSize   = 0f;
        style.PopupBorderSize   = 1f;
        style.TabBorderSize     = 0f;

        style.WindowPadding      = new Vector2(12f, 12f);
        style.FramePadding       = new Vector2(10f, 6f);
        style.ItemSpacing        = new Vector2(8f, 7f);
        style.ItemInnerSpacing   = new Vector2(6f, 4f);
        style.IndentSpacing      = 20f;
        style.ScrollbarSize      = 12f;
        style.GrabMinSize        = 10f;

        style.WindowTitleAlign   = new Vector2(0.5f, 0.5f);
        style.AntiAliasedLines   = true;
        style.AntiAliasedFill    = true;

        colors[(int)ImGuiCol.WindowBg]           = BgBase;
        colors[(int)ImGuiCol.ChildBg]            = BgSurface;
        colors[(int)ImGuiCol.PopupBg]            = BgElevated;

        colors[(int)ImGuiCol.Border]             = Border;
        colors[(int)ImGuiCol.BorderShadow]       = Vector4.Zero;

        colors[(int)ImGuiCol.TitleBg]            = BgDeep;
        colors[(int)ImGuiCol.TitleBgActive]      = BgSurface;
        colors[(int)ImGuiCol.TitleBgCollapsed]   = BgDeep;

        colors[(int)ImGuiCol.Text]               = TextPrimary;
        colors[(int)ImGuiCol.TextDisabled]       = TextDisabled;
        colors[(int)ImGuiCol.TextSelectedBg]     = CyanGlow;

        colors[(int)ImGuiCol.FrameBg]            = BgElevated;
        colors[(int)ImGuiCol.FrameBgHovered]     = BgHover;
        colors[(int)ImGuiCol.FrameBgActive]      = CyanMuted;

        colors[(int)ImGuiCol.Button]             = BgElevated;
        colors[(int)ImGuiCol.ButtonHovered]      = BgHover;
        colors[(int)ImGuiCol.ButtonActive]       = CyanMuted;

        colors[(int)ImGuiCol.Header]             = CyanMuted;
        colors[(int)ImGuiCol.HeaderHovered]      = CyanGlow;
        colors[(int)ImGuiCol.HeaderActive]       = HexA(0x9B30FF, 0.45f);

        colors[(int)ImGuiCol.CheckMark]          = Cyan;
        colors[(int)ImGuiCol.SliderGrab]         = CyanDim;
        colors[(int)ImGuiCol.SliderGrabActive]   = Cyan;

        colors[(int)ImGuiCol.ScrollbarBg]          = ScrollBg;
        colors[(int)ImGuiCol.ScrollbarGrab]        = ScrollGrab;
        colors[(int)ImGuiCol.ScrollbarGrabHovered] = ScrollHover;
        colors[(int)ImGuiCol.ScrollbarGrabActive]  = ScrollActive;

        colors[(int)ImGuiCol.Separator]          = Border;
        colors[(int)ImGuiCol.SeparatorHovered]   = CyanDim;
        colors[(int)ImGuiCol.SeparatorActive]    = Cyan;

        colors[(int)ImGuiCol.ResizeGrip]         = Vector4.Zero;
        colors[(int)ImGuiCol.ResizeGripHovered]  = Vector4.Zero;
        colors[(int)ImGuiCol.ResizeGripActive]   = Vector4.Zero;

        colors[(int)ImGuiCol.Tab]                = BgSurface;
        colors[(int)ImGuiCol.TabHovered]         = CyanMuted;
        colors[(int)ImGuiCol.TabSelected]        = CyanGlow;

        colors[(int)ImGuiCol.TableHeaderBg]      = BgSurface;
        colors[(int)ImGuiCol.TableBorderStrong]  = Border;
        colors[(int)ImGuiCol.TableBorderLight]   = HexA(0xFFFFFF, 0.04f);

        colors[(int)ImGuiCol.ModalWindowDimBg]   = HexA(0x000000, 0.60f);
        colors[(int)ImGuiCol.NavWindowingHighlight] = Cyan;
    }
    
    public static Vector4 Hex(uint rgb)
    {
        float r = ((rgb >> 16) & 0xFF) / 255f;
        float g = ((rgb >> 8)  & 0xFF) / 255f;
        float b = ( rgb        & 0xFF) / 255f;
        return new Vector4(r, g, b, 1f);
    }

    public static Vector4 HexA(uint rgb, float a)
    {
        float r = ((rgb >> 16) & 0xFF) / 255f;
        float g = ((rgb >> 8)  & 0xFF) / 255f;
        float b = ( rgb        & 0xFF) / 255f;
        return new Vector4(r, g, b, a);
    }

    public static uint ToU32(Vector4 c) => ImGuiNET.ImGui.ColorConvertFloat4ToU32(c);
    public static Vector4 WithAlpha(Vector4 c, float a) => new(c.X, c.Y, c.Z, a);
}