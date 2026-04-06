using System.Collections.Concurrent;
using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices;
using HarmonyLib;
using ImGuiNET;

[HarmonyPatch]
public static class ImGuiInputHook
{
    private static readonly ConcurrentQueue<Action<ImGuiIOPtr>> _ioQueue = new();

    public static void EnqueueIO(Action<ImGuiIOPtr> action) => _ioQueue.Enqueue(action);

    public static volatile bool WantCaptureMouse = false;
    public static volatile bool WantCaptureKeyboard = false;

    public static void FlushEvents()
    {
        if (!ImGuiRenderer.OverlayVisible)
        {
            while (_ioQueue.TryDequeue(out _)) { }
            return;
        }
        var io = ImGui.GetIO();
        while (_ioQueue.TryDequeue(out var action))
            action(io);
    }

    public static void SnapshotWantCapture()
    {
        if (!ImGuiRenderer.OverlayVisible)
        {
            WantCaptureMouse = false;
            WantCaptureKeyboard = false;
            return;
        }
        var io = ImGui.GetIO();
        WantCaptureMouse = io.WantCaptureMouse;
        WantCaptureKeyboard = io.WantCaptureKeyboard;
    }

    static IEnumerable<MethodBase> TargetMethods()
    {
        return
        [
            ..ResolveInputManagerMethods(),
            ..ResolveUiManagerMethods(),
            ..ResolveGameControllerMethods(),
        ];
    }

    private static IEnumerable<MethodBase> ResolveInputManagerMethods()
    {
        var t = AccessTools.TypeByName("Robust.Client.Input.InputManager");
        if (t is null)
        {
            MarseyLogger.Fatal("[ImGuiInputHook] InputManager type not found.");
            yield break;
        }
        foreach (var name in new[] { "KeyDown", "KeyUp" })
        {
            var m = AccessTools.Method(t, name);
            if (m is null) { MarseyLogger.Warn($"[ImGuiInputHook] InputManager.{name} not found."); continue; }
            MarseyLogger.Info($"[ImGuiInputHook] Patching {m.FullDescription()}");
            yield return m;
        }
    }

    private static IEnumerable<MethodBase> ResolveUiManagerMethods()
    {
        var t = AccessTools.TypeByName("Robust.Client.UserInterface.UserInterfaceManager");
        if (t is null)
        {
            MarseyLogger.Fatal("[ImGuiInputHook] UserInterfaceManager type not found.");
            yield break;
        }
        foreach (var name in new[] { "MouseMove", "MouseWheel" })
        {
            var m = AccessTools.Method(t, name);
            if (m is null) { MarseyLogger.Warn($"[ImGuiInputHook] UserInterfaceManager.{name} not found."); continue; }
            MarseyLogger.Info($"[ImGuiInputHook] Patching {m.FullDescription()}");
            yield return m;
        }
    }

    private static IEnumerable<MethodBase> ResolveGameControllerMethods()
    {
        var t = AccessTools.TypeByName("Robust.Client.GameController");
        if (t is null)
        {
            MarseyLogger.Fatal("[ImGuiInputHook] GameController type not found.");
            yield break;
        }
        var m = AccessTools.Method(t, "TextEntered");
        if (m is null) { MarseyLogger.Warn("[ImGuiInputHook] GameController.TextEntered not found."); yield break; }
        MarseyLogger.Info($"[ImGuiInputHook] Patching {m.FullDescription()}");
        yield return m;
    }

    [HarmonyPrefix]
    static bool Prefix(MethodBase __originalMethod, object[] __args)
    {
        try
        {
            return __originalMethod.Name switch
            {
                "KeyDown" => OnKeyDown(__args[0]),
                "KeyUp" => OnKeyUp(__args[0]),
                "MouseMove" => OnMouseMove(__args[0]),
                "MouseWheel" => OnMouseWheel(__args[0]),
                "TextEntered" => OnTextEntered(__args[0]),
                _ => true
            };
        }
        catch (Exception ex)
        {
            MarseyLogger.Warn($"[ImGuiInputHook] Exception in {__originalMethod.Name}: {ex.Message}");
            return true;
        }
    }

    private static readonly HashSet<byte> _heldBeforeOverlay = new();

    private static bool OnKeyDown(object args)
    {
        var key = ReadField(args, "Key") ?? ReadProperty(args, "Key");
        if (key is null) return true;

        var keyByte = Convert.ToByte(key);

        if (keyByte == 83)
        {
            if (!ImGuiRenderer.OverlayVisible)
                _heldBeforeOverlay.Add(keyByte);

            ImGuiRenderer.OverlayVisible = !ImGuiRenderer.OverlayVisible;
            MarseyLogger.Info($"[ImGuiInputHook] Overlay → {ImGuiRenderer.OverlayVisible}");
            return true;
        }

        if (!ImGuiRenderer.OverlayVisible)
        {
            _heldBeforeOverlay.Add(keyByte);
            return true;
        }

        if (KeyMap.IsMouseKey(key))
        {
            var btnIdx = KeyMap.MouseKeyToImGuiButton(key);
            if (btnIdx >= 0)
                _ioQueue.Enqueue(io => io.MouseDown[btnIdx] = true);
            return false;
        }

        var imKey = KeyMap.ToImGuiKey(key);
        var ctrl = Convert.ToBoolean(ReadField(args, "Control") ?? ReadProperty(args, "Control") ?? false);
        var shift = Convert.ToBoolean(ReadField(args, "Shift") ?? ReadProperty(args, "Shift") ?? false);
        var alt = Convert.ToBoolean(ReadField(args, "Alt") ?? ReadProperty(args, "Alt") ?? false);
        var super = Convert.ToBoolean(ReadField(args, "System") ?? ReadProperty(args, "System") ?? false);

        _ioQueue.Enqueue(io =>
        {
            io.AddKeyEvent(ImGuiKey.ModCtrl, ctrl);
            io.AddKeyEvent(ImGuiKey.ModShift, shift);
            io.AddKeyEvent(ImGuiKey.ModAlt, alt);
            io.AddKeyEvent(ImGuiKey.ModSuper, super);
            if (imKey != ImGuiKey.None)
                io.AddKeyEvent(imKey, true);
        });

        return false;
    }

    private static bool OnKeyUp(object args)
    {
        var key = ReadField(args, "Key") ?? ReadProperty(args, "Key");
        if (key is null) return true;

        var keyByte = Convert.ToByte(key);

        if (!ImGuiRenderer.OverlayVisible)
        {
            _heldBeforeOverlay.Remove(keyByte);
            return true;
        }

        if (_heldBeforeOverlay.Remove(keyByte))
            return true;

        if (KeyMap.IsMouseKey(key))
        {
            var btnIdx = KeyMap.MouseKeyToImGuiButton(key);
            if (btnIdx >= 0)
                _ioQueue.Enqueue(io => io.MouseDown[btnIdx] = false);
            return false;
        }

        var imKey = KeyMap.ToImGuiKey(key);
        var ctrl = Convert.ToBoolean(ReadField(args, "Control") ?? ReadProperty(args, "Control") ?? false);
        var shift = Convert.ToBoolean(ReadField(args, "Shift") ?? ReadProperty(args, "Shift") ?? false);
        var alt = Convert.ToBoolean(ReadField(args, "Alt") ?? ReadProperty(args, "Alt") ?? false);
        var super = Convert.ToBoolean(ReadField(args, "System") ?? ReadProperty(args, "System") ?? false);

        _ioQueue.Enqueue(io =>
        {
            io.AddKeyEvent(ImGuiKey.ModCtrl, ctrl);
            io.AddKeyEvent(ImGuiKey.ModShift, shift);
            io.AddKeyEvent(ImGuiKey.ModAlt, alt);
            io.AddKeyEvent(ImGuiKey.ModSuper, super);
            if (imKey != ImGuiKey.None)
                io.AddKeyEvent(imKey, false);
        });

        return false;
    }

    private static bool OnMouseMove(object args)
    {
        if (!ImGuiRenderer.OverlayVisible) return true;

        var screenCoords = ReadField(args, "Position") ?? ReadProperty(args, "Position");
        Vector2 pos = default;

        if (screenCoords is not null)
        {
            var innerPos = ReadField(screenCoords, "Position") ?? ReadProperty(screenCoords, "Position");
            if (innerPos is Vector2 v) pos = v;
        }

        _ioQueue.Enqueue(io => io.MousePos = pos);
        return !WantCaptureMouse;
    }

    private static bool OnMouseWheel(object args)
    {
        if (!ImGuiRenderer.OverlayVisible) return true;

        var delta = ReadField(args, "Delta") ?? ReadProperty(args, "Delta");
        Vector2 d = default;
        if (delta is Vector2 v) d = v;

        _ioQueue.Enqueue(io =>
        {
            io.MouseWheel += d.Y;
            io.MouseWheelH += d.X;
        });

        return !WantCaptureMouse;
    }

    private static bool OnTextEntered(object args)
    {
        if (!ImGuiRenderer.OverlayVisible) return true;

        var textObj = ReadField(args, "Text") ?? ReadProperty(args, "Text");

        string? captured = null;

        if (textObj is string s)
            captured = s;
        else if (textObj is char c)
            captured = c.ToString();

        if (string.IsNullOrEmpty(captured)) return !WantCaptureKeyboard;

        var cap = captured;
        _ioQueue.Enqueue(io =>
        {
            foreach (var rune in cap.EnumerateRunes())
                io.AddInputCharacter((uint)rune.Value);
        });

        return !WantCaptureKeyboard;
    }

    private static readonly ConcurrentDictionary<(Type, string), FieldInfo?> _fieldCache = new();
    private static readonly ConcurrentDictionary<(Type, string), PropertyInfo?> _propCache = new();

    private static object? ReadField(object obj, string name)
    {
        var type = obj.GetType();
        var fi = _fieldCache.GetOrAdd((type, name), k =>
            k.Item1.GetField(k.Item2,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic));
        return fi?.GetValue(obj);
    }

    private static object? ReadProperty(object obj, string name)
    {
        var type = obj.GetType();
        var pi = _propCache.GetOrAdd((type, name), k =>
            k.Item1.GetProperty(k.Item2,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic));
        return pi?.GetValue(obj);
    }
}

[HarmonyPatch]
public static class ImGuiSdl3TextInputPatch
{
    private static bool _loggedActive;

    [HarmonyTargetMethod]
    private static MethodBase? TargetMethod()
    {
        var t = AccessTools.TypeByName("Robust.Client.Graphics.Clyde.Clyde+Sdl3WindowingImpl");
        if (t is null)
        {
            MarseyLogger.Warn("[ImGuiSdl3TextInputPatch] Sdl3WindowingImpl not found — GLFW backend? Skipped.");
            return null;
        }
        var m = AccessTools.Method(t, "ProcessSdl3EventTextInput");
        MarseyLogger.Info($"[ImGuiSdl3TextInputPatch] Target: {m?.FullDescription() ?? "NOT FOUND"}");
        return m;
    }

    [HarmonyPrefix]
    private static bool Prefix(object[] __args)
    {
        try
        {
            if (!ImGuiRenderer.OverlayVisible) return true;

            if (!_loggedActive)
            {
                _loggedActive = true;
                MarseyLogger.Info("[ImGuiSdl3TextInputPatch] First SDL3 TEXT_INPUT event captured.");
            }

            if (__args is null || __args.Length < 1 || __args[0] is null) return true;

            var ev = __args[0];
            var textField = AccessTools.Field(ev.GetType(), "text");
            if (textField is null) return true;

            var textPtrObj = textField.GetValue(ev);
            if (textPtrObj is null) return true;

            nint ptr = textPtrObj switch
            {
                IntPtr ip => ip,
                long l => (nint)l,
                ulong ul => unchecked((nint)ul),
                _ => nint.Zero
            };

            if (ptr == nint.Zero) return true;

            var s = Marshal.PtrToStringUTF8((IntPtr)ptr);
            if (string.IsNullOrEmpty(s)) return true;

            foreach (var rune in s.EnumerateRunes())
            {
                var cp = (uint)rune.Value;
                ImGuiInputHook.EnqueueIO(io => io.AddInputCharacter(cp));
            }

            return !ImGuiInputHook.WantCaptureKeyboard;
        }
        catch (Exception ex)
        {
            MarseyLogger.Warn($"[ImGuiSdl3TextInputPatch] {ex.Message}");
            return true;
        }
    }
}

[HarmonyPatch]
public static class ImGuiSdl3TextInputStopPatch
{
    private static bool _loggedBlock;

    [HarmonyTargetMethod]
    private static MethodBase? TargetMethod()
    {
        var t = AccessTools.TypeByName("Robust.Client.Graphics.Clyde.Clyde+Sdl3WindowingImpl");
        if (t is null) return null;

        var m = AccessTools.Method(t, "TextInputStop")
             ?? AccessTools.Method(t, "StopTextInput");

        MarseyLogger.Info($"[ImGuiSdl3TextInputStopPatch] Target: {m?.FullDescription() ?? "NOT FOUND"}");
        return m;
    }

    [HarmonyPrefix]
    private static bool Prefix()
    {
        if (ImGuiRenderer.OverlayVisible && ImGuiInputHook.WantCaptureKeyboard)
        {
            if (!_loggedBlock)
            {
                _loggedBlock = true;
                MarseyLogger.Info("[ImGuiSdl3TextInputStopPatch] Blocking TextInputStop — ImGui has focus.");
            }
            return false;
        }
        _loggedBlock = false;
        return true;
    }
}