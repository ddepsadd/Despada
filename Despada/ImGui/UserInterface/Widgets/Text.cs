using System.Numerics;
using ImGuiNET;

namespace Despada.ImGui.UserInterface;

public static partial class Widgets
{
    public static void DrawGradientText(ImDrawListPtr dl, ImFontPtr font, float fontSize,
        Vector2 pos, string text, Vector4 colorStart, Vector4 colorMid, Vector4 colorEnd,
        float shear = 0f)
    {
        if (string.IsNullOrEmpty(text))
            return;

        var x = pos.X;
        var lastIdx = text.Length - 1;
        var charH = font.CalcTextSizeA(fontSize, float.MaxValue, 0f, "A").Y;

        for (int i = 0; i < text.Length; i++)
        {
            var t = lastIdx > 0 ? (float)i / lastIdx : 0f;

            Vector4 color;
            if (t < 0.5f)
            {
                var lt = t * 2f;
                color = colorStart + (colorMid - colorStart) * lt;
            }
            else
            {
                var lt = (t - 0.5f) * 2f;
                color = colorMid + (colorEnd - colorMid) * lt;
            }

            var ch = text[i].ToString();
            var charW = font.CalcTextSizeA(fontSize, float.MaxValue, 0f, ch).X;

            if (shear != 0f)
            {
                var shearOffset = shear * charH;
                dl.AddText(font, fontSize, new Vector2(x + shearOffset, pos.Y), Theme.ToU32(color), ch);
            }
            else
            {
                dl.AddText(font, fontSize, new Vector2(x, pos.Y), Theme.ToU32(color), ch);
            }

            x += charW;
        }
    }
}