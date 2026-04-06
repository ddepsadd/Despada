using ImGuiNET;

internal static class KeyMap
{
    private static readonly ImGuiKey[] _table = BuildTable();
    public static ImGuiKey ToImGuiKey(object robustKey)
    {
        var idx = Convert.ToByte(robustKey);
        return _table[idx];
    }

    public static bool IsMouseKey(object robustKey)
    {
        var idx = Convert.ToByte(robustKey);
        return idx is >= 1 and <= 9;
    }

    public static int MouseKeyToImGuiButton(object robustKey)
    {
        return Convert.ToByte(robustKey) switch
        {
            1 => 0,
            2 => 1,
            3 => 2,
            4 => 3,
            5 => 4,
            _ => -1
        };
    }

    private static ImGuiKey[] BuildTable()
    {
        var t = new ImGuiKey[256];
        
        Array.Fill(t, ImGuiKey.None);

        t[10] = ImGuiKey.A; t[11] = ImGuiKey.B; t[12] = ImGuiKey.C;
        t[13] = ImGuiKey.D; t[14] = ImGuiKey.E; t[15] = ImGuiKey.F;
        t[16] = ImGuiKey.G; t[17] = ImGuiKey.H; t[18] = ImGuiKey.I;
        t[19] = ImGuiKey.J; t[20] = ImGuiKey.K; t[21] = ImGuiKey.L;
        t[22] = ImGuiKey.M; t[23] = ImGuiKey.N; t[24] = ImGuiKey.O;
        t[25] = ImGuiKey.P; t[26] = ImGuiKey.Q; t[27] = ImGuiKey.R;
        t[28] = ImGuiKey.S; t[29] = ImGuiKey.T; t[30] = ImGuiKey.U;
        t[31] = ImGuiKey.V; t[32] = ImGuiKey.W; t[33] = ImGuiKey.X;
        t[34] = ImGuiKey.Y; t[35] = ImGuiKey.Z;

        t[36] = ImGuiKey._0; t[37] = ImGuiKey._1; t[38] = ImGuiKey._2;
        t[39] = ImGuiKey._3; t[40] = ImGuiKey._4; t[41] = ImGuiKey._5;
        t[42] = ImGuiKey._6; t[43] = ImGuiKey._7; t[44] = ImGuiKey._8;
        t[45] = ImGuiKey._9;

        t[46] = ImGuiKey.Keypad0; t[47] = ImGuiKey.Keypad1; t[48] = ImGuiKey.Keypad2;
        t[49] = ImGuiKey.Keypad3; t[50] = ImGuiKey.Keypad4; t[51] = ImGuiKey.Keypad5;
        t[52] = ImGuiKey.Keypad6; t[53] = ImGuiKey.Keypad7; t[54] = ImGuiKey.Keypad8;
        t[55] = ImGuiKey.Keypad9;

        t[56] = ImGuiKey.Escape;
        t[57] = ImGuiKey.LeftCtrl;
        t[58] = ImGuiKey.LeftShift;
        t[59] = ImGuiKey.LeftAlt;
        t[60] = ImGuiKey.LeftSuper;
        t[61] = ImGuiKey.RightSuper;
        t[62] = ImGuiKey.Menu;
        t[63] = ImGuiKey.LeftBracket;
        t[64] = ImGuiKey.RightBracket;
        t[65] = ImGuiKey.Semicolon;
        t[66] = ImGuiKey.Comma;
        t[67] = ImGuiKey.Period;
        t[68] = ImGuiKey.Apostrophe;
        t[69] = ImGuiKey.Slash;
        t[70] = ImGuiKey.Backslash;
        t[71] = ImGuiKey.GraveAccent;
        t[72] = ImGuiKey.Equal;
        t[73] = ImGuiKey.Space;
        t[74] = ImGuiKey.Enter;
        t[75] = ImGuiKey.KeypadEnter;
        t[76] = ImGuiKey.Backspace;
        t[77] = ImGuiKey.Tab;
        t[78] = ImGuiKey.PageUp;
        t[79] = ImGuiKey.PageDown;
        t[80] = ImGuiKey.End;
        t[81] = ImGuiKey.Home;
        t[82] = ImGuiKey.Insert;
        t[83] = ImGuiKey.Delete;
        t[84] = ImGuiKey.Minus;
        t[85] = ImGuiKey.KeypadAdd;
        t[86] = ImGuiKey.KeypadSubtract;
        t[87] = ImGuiKey.KeypadDivide;
        t[88] = ImGuiKey.KeypadMultiply;
        t[89] = ImGuiKey.KeypadDecimal;

        t[90] = ImGuiKey.LeftArrow;
        t[91] = ImGuiKey.RightArrow;
        t[92] = ImGuiKey.UpArrow;
        t[93] = ImGuiKey.DownArrow;

        t[94]  = ImGuiKey.F1;  t[95]  = ImGuiKey.F2;  t[96]  = ImGuiKey.F3;
        t[97]  = ImGuiKey.F4;  t[98]  = ImGuiKey.F5;  t[99]  = ImGuiKey.F6;
        t[100] = ImGuiKey.F7;  t[101] = ImGuiKey.F8;  t[102] = ImGuiKey.F9;
        t[103] = ImGuiKey.F10; t[104] = ImGuiKey.F11; t[105] = ImGuiKey.F12;

        t[118] = ImGuiKey.Pause;

        return t;
    }
}