using System.Numerics;
using ImGuiNET;

namespace Despada.ImGui.UserInterface;

public static partial class Widgets
{
    private static readonly Dictionary<string, int> _comboPage = new();

    public static bool Combo(string label, ref int current, string items)
    {
        var parsed = items.Split('\0', StringSplitOptions.RemoveEmptyEntries);
        return Combo(label, ref current, parsed);
    }

    public static bool Combo(string label, ref int current, string[] items)
    {
        const float boxH     = 34f;
        const float boxW     = 160f;
        const float boxR     = 6f;
        const float pad      = 14f;
        const float stripH   = 36f;
        const float stripR   = 6f;
        const float closeW   = 38f;
        const float arrowW   = 28f;
        const float itemMin  = 72f;
        const float sepInset = 8f;
        const int   maxVis   = 4;

        ImGuiNET.ImGui.Spacing();

        var dl   = ImGuiNET.ImGui.GetWindowDrawList();
        var font = ImGuiNET.ImGui.GetFont();
        var fs   = font.FontSize;
        var cw   = ImGuiNET.ImGui.GetContentRegionAvail().X;
        var pos  = ImGuiNET.ImGui.GetCursorScreenPos();
        var changed = false;

        var dLabel  = label.Contains("##") ? label[..label.IndexOf("##")] : label;
        var pid     = $"{label}_p";
        bool isOpen = ImGuiNET.ImGui.IsPopupOpen(pid);
        var openT   = GetToggleT($"{label}_open", isOpen);

        if (!string.IsNullOrEmpty(dLabel))
        {
            ImGuiNET.ImGui.AlignTextToFramePadding();
            ImGuiNET.ImGui.TextUnformatted(dLabel);
        }

        ImGuiNET.ImGui.SameLine(cw - boxW);
        pos = ImGuiNET.ImGui.GetCursorScreenPos();

        var bMin = pos;
        var bMax = new Vector2(pos.X + boxW, pos.Y + boxH);

        ImGuiNET.ImGui.InvisibleButton($"{label}_b", new Vector2(boxW, boxH));
        bool bH = ImGuiNET.ImGui.IsItemHovered();
        if (bH && ImGuiNET.ImGui.IsMouseClicked(ImGuiMouseButton.Left))
        {
            ImGuiNET.ImGui.OpenPopup(pid);
            _comboPage[label] = current >= maxVis ? current - maxVis + 1 : 0;
        }

        dl.AddRectFilled(bMin, bMax, Theme.ToU32(SectionBg), boxR);

        var baseBdr = bH ? Theme.BorderHover : Theme.Border;
        var bdrCol = new Vector4(
            baseBdr.X + (Theme.BorderActive.X - baseBdr.X) * openT,
            baseBdr.Y + (Theme.BorderActive.Y - baseBdr.Y) * openT,
            baseBdr.Z + (Theme.BorderActive.Z - baseBdr.Z) * openT,
            baseBdr.W + (Theme.BorderActive.W - baseBdr.W) * openT);

        dl.AddRect(bMin, bMax, Theme.ToU32(bdrCol), boxR, ImDrawFlags.None, 1f);

        var vt = current >= 0 && current < items.Length ? items[current] : "";
        var vs = font.CalcTextSizeA(fs, float.MaxValue, 0f, vt);
        dl.AddText(font, fs,
            new Vector2(bMin.X + pad, bMin.Y + (boxH - vs.Y) * 0.5f),
            Theme.ToU32(Theme.TextPrimary), vt);

        var chev = isOpen ? "<" : ">";
        var chSz = font.CalcTextSizeA(fs, float.MaxValue, 0f, chev);
        dl.AddText(font, fs,
            new Vector2(bMax.X - pad - chSz.X, bMin.Y + (boxH - chSz.Y) * 0.5f),
            Theme.ToU32(isOpen ? Theme.TextPrimary : Theme.TextSecondary), chev);

        bool needPaging = items.Length > maxVis;
        _comboPage.TryGetValue(label, out int pageOff);
        int maxOff = Math.Max(0, items.Length - maxVis);
        pageOff = Math.Clamp(pageOff, 0, maxOff);
        int visN = Math.Min(items.Length, maxVis);

        float maxTW = 0f;
        for (int i = 0; i < items.Length; i++)
        {
            var tw = font.CalcTextSizeA(fs, float.MaxValue, 0f, items[i]).X;
            if (tw > maxTW) maxTW = tw;
        }

        var iW     = MathF.Max(maxTW + 28f, itemMin);
        var totalW = closeW + iW * visN + (needPaging ? arrowW * 2f : 0f);
        var sepCol = Theme.ToU32(Theme.HexA(0xFFFFFF, 0.06f));

        ImGuiNET.ImGui.SetNextWindowPos(new Vector2(bMin.X, bMax.Y + 1f));
        ImGuiNET.ImGui.SetNextWindowSize(new Vector2(totalW, stripH));

        ImGuiNET.ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.Zero);
        ImGuiNET.ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, stripR);
        ImGuiNET.ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 1f);
        ImGuiNET.ImGui.PushStyleColor(ImGuiCol.PopupBg, Theme.BgElevated);
        ImGuiNET.ImGui.PushStyleColor(ImGuiCol.Border, Theme.HexA(0xFFFFFF, 0.10f));

        if (ImGuiNET.ImGui.BeginPopup(pid,
            ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize))
        {
            var p  = ImGuiNET.ImGui.GetWindowDrawList();
            var wP = ImGuiNET.ImGui.GetWindowPos();
            ImGuiNET.ImGui.Dummy(new Vector2(totalW, stripH));

            float curX = wP.X;

            var xMin = new Vector2(curX, wP.Y);
            var xMax = new Vector2(curX + closeW, wP.Y + stripH);
            bool xHov = ImGuiNET.ImGui.IsMouseHoveringRect(xMin, xMax);
            if (xHov)
                p.AddRectFilled(xMin, xMax, Theme.ToU32(Theme.BgHover), stripR, ImDrawFlags.RoundCornersLeft);

            var xTs = font.CalcTextSizeA(fs, float.MaxValue, 0f, "X");
            p.AddText(font, fs,
                new Vector2(xMin.X + (closeW - xTs.X) * 0.5f, xMin.Y + (stripH - xTs.Y) * 0.5f),
                Theme.ToU32(xHov ? Theme.TextPrimary : Theme.TextSecondary), "X");

            if (xHov && ImGuiNET.ImGui.IsMouseClicked(ImGuiMouseButton.Left))
                ImGuiNET.ImGui.CloseCurrentPopup();

            p.AddLine(new Vector2(xMax.X, wP.Y + sepInset), new Vector2(xMax.X, wP.Y + stripH - sepInset), sepCol);
            curX += closeW;

            if (needPaging)
            {
                var aMin = new Vector2(curX, wP.Y);
                var aMax = new Vector2(curX + arrowW, wP.Y + stripH);
                bool aH = ImGuiNET.ImGui.IsMouseHoveringRect(aMin, aMax);
                bool canL = pageOff > 0;

                if (aH && canL)
                    p.AddRectFilled(aMin, aMax, Theme.ToU32(Theme.BgHover));

                var aTs = font.CalcTextSizeA(fs, float.MaxValue, 0f, "<");
                p.AddText(font, fs,
                    new Vector2(aMin.X + (arrowW - aTs.X) * 0.5f, aMin.Y + (stripH - aTs.Y) * 0.5f),
                    Theme.ToU32(canL ? Theme.TextSecondary : Theme.TextDisabled), "<");

                if (aH && canL && ImGuiNET.ImGui.IsMouseClicked(ImGuiMouseButton.Left))
                    _comboPage[label] = --pageOff;

                p.AddLine(new Vector2(aMax.X, wP.Y + sepInset), new Vector2(aMax.X, wP.Y + stripH - sepInset), sepCol);
                curX += arrowW;
            }

            for (int vi = 0; vi < visN; vi++)
            {
                int i = vi + pageOff;
                if (i >= items.Length) break;

                var oMin = new Vector2(curX, wP.Y);
                var oMax = new Vector2(curX + iW, wP.Y + stripH);
                bool oHov = ImGuiNET.ImGui.IsMouseHoveringRect(oMin, oMax);
                bool sel  = i == current;

                var cf = (vi == visN - 1 && !needPaging) ? ImDrawFlags.RoundCornersRight : ImDrawFlags.None;

                if (sel)
                {
                    p.AddRectFilled(oMin, oMax, Theme.ToU32(Theme.HexA(0x6B2FD6, 0.25f)), stripR, cf);
                    p.AddRectFilledMultiColor(
                        new Vector2(oMin.X + 2f, oMin.Y),
                        new Vector2(oMax.X - 2f, oMax.Y),
                        Theme.ToU32(Theme.HexA(0x6B2FD6, 0.20f)),
                        Theme.ToU32(Theme.HexA(0x9B30FF, 0.30f)),
                        Theme.ToU32(Theme.HexA(0x9B30FF, 0.30f)),
                        Theme.ToU32(Theme.HexA(0x6B2FD6, 0.20f)));
                }
                else if (oHov)
                {
                    p.AddRectFilled(oMin, oMax, Theme.ToU32(Theme.BgHover), stripR, cf);
                }

                var ts = font.CalcTextSizeA(fs, float.MaxValue, 0f, items[i]);
                p.AddText(font, fs,
                    new Vector2(oMin.X + (iW - ts.X) * 0.5f, oMin.Y + (stripH - ts.Y) * 0.5f),
                    Theme.ToU32(sel ? Theme.Cyan : Theme.TextPrimary), items[i]);

                if (vi < visN - 1)
                    p.AddLine(new Vector2(oMax.X, wP.Y + sepInset), new Vector2(oMax.X, wP.Y + stripH - sepInset), sepCol);

                if (oHov && ImGuiNET.ImGui.IsMouseClicked(ImGuiMouseButton.Left))
                {
                    current = i;
                    changed = true;
                    ImGuiNET.ImGui.CloseCurrentPopup();
                }

                curX += iW;
            }

            if (needPaging)
            {
                p.AddLine(new Vector2(curX, wP.Y + sepInset), new Vector2(curX, wP.Y + stripH - sepInset), sepCol);

                var aMin = new Vector2(curX, wP.Y);
                var aMax = new Vector2(curX + arrowW, wP.Y + stripH);
                bool aH = ImGuiNET.ImGui.IsMouseHoveringRect(aMin, aMax);
                bool canR = pageOff < maxOff;

                if (aH && canR)
                    p.AddRectFilled(aMin, aMax, Theme.ToU32(Theme.BgHover), stripR, ImDrawFlags.RoundCornersRight);

                var aTs = font.CalcTextSizeA(fs, float.MaxValue, 0f, ">");
                p.AddText(font, fs,
                    new Vector2(aMin.X + (arrowW - aTs.X) * 0.5f, aMin.Y + (stripH - aTs.Y) * 0.5f),
                    Theme.ToU32(canR ? Theme.TextSecondary : Theme.TextDisabled), ">");

                if (aH && canR && ImGuiNET.ImGui.IsMouseClicked(ImGuiMouseButton.Left))
                    _comboPage[label] = ++pageOff;
            }

            ImGuiNET.ImGui.EndPopup();
        }

        ImGuiNET.ImGui.PopStyleColor(2);
        ImGuiNET.ImGui.PopStyleVar(3);

        ImGuiNET.ImGui.Spacing();
        return changed;
    }
}