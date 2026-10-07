// SPDX-FileCopyrightText: 2026 ddepsadd <https://github.com/ddepsadd>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Collections.Concurrent;
using System.Numerics;
using System.Reflection;
using HarmonyLib;
using ImGuiNET;

namespace Despada.ImGui.Hook;

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
        var io = ImGuiNET.ImGui.GetIO();
        while (_ioQueue.TryDequeue(out var action))
            action(io);
    }

    public static void Reset()
    {
        while (_ioQueue.TryDequeue(out _)) { }
        _gameHeld.Clear();
        WantCaptureMouse    = false;
        WantCaptureKeyboard = false;
    }

    public static void SnapshotWantCapture()
    {
        if (!ImGuiRenderer.OverlayVisible)
        {
            WantCaptureMouse = false;
            WantCaptureKeyboard = false;
            return;
        }
        var io = ImGuiNET.ImGui.GetIO();
        WantCaptureMouse = io.WantCaptureMouse;
        // io.WantCaptureKeyboard is also true while any item is active (dragging a window,
        // holding a slider...), which would freeze the game's keyboard. Only an active text
        // field really owns the keyboard; ImGui still receives every key either way.
        WantCaptureKeyboard = io.WantTextInput;
    }

    private static List<MethodBase>? _targets;

    private static List<MethodBase> Targets => _targets ??=
    [
        ..ResolveInputManagerMethods(),
        ..ResolveUiManagerMethods(),
        ..ResolveGameControllerMethods(),
    ];

    // Harmony throws on an empty TargetMethods(); decline the patch instead.
    static bool Prepare()
    {
        if (Targets.Count > 0) return true;
        MarseyLogger.Fatal("[ImGuiInputHook] No input targets found — input hook disabled.");
        return false;
    }

    static IEnumerable<MethodBase> TargetMethods() => Targets;

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

    // Keys/buttons whose key-down went to the game: the matching key-up must go there too,
    // even if the overlay opened or ImGui grabbed focus in between.
    private static readonly HashSet<byte> _gameHeld = new();

    private const string ToggleKeyName = "Delete";
    private static byte? _toggleKey;

    // Resolved by name: the engine's Keyboard.Key values shift whenever a key is inserted.
    private static byte ToggleKey => _toggleKey ??= ResolveKey(ToggleKeyName);

    private static byte ResolveKey(string name)
    {
        var keyType = AccessTools.TypeByName("Robust.Client.Input.Keyboard+Key");
        if (keyType is not null && Enum.TryParse(keyType, name, out var value))
            return Convert.ToByte(value);

        MarseyLogger.Warn($"[ImGuiInputHook] Keyboard.Key.{name} not found, falling back to 83.");
        return 83;
    }

    private static bool OnKeyDown(object args)
    {
        var key = Read(args, "Key");
        if (key is null) return true;

        var keyByte = Convert.ToByte(key);

        if (keyByte == ToggleKey)
        {
            if (ImGuiRenderer.Failed) return true;

            if (Read(args, "IsRepeat") is not true)
            {
                ImGuiRenderer.OverlayVisible = !ImGuiRenderer.OverlayVisible;
                MarseyLogger.Info($"[ImGuiInputHook] Overlay → {ImGuiRenderer.OverlayVisible}");
            }
            // The toggle key belongs to the overlay: neither the game nor ImGui sees it.
            return false;
        }

        if (!ImGuiRenderer.OverlayVisible)
        {
            _gameHeld.Add(keyByte);
            return true;
        }

        // ImGui always sees the event so its state stays consistent; the game gets it
        // unless ImGui wants this kind of input (hovered window / active text field).
        var isMouse = EnqueueKey(args, key, down: true);
        if (isMouse ? WantCaptureMouse : WantCaptureKeyboard)
            return false;

        _gameHeld.Add(keyByte);
        return true;
    }

    private static bool OnKeyUp(object args)
    {
        var key = Read(args, "Key");
        if (key is null) return true;

        var keyByte = Convert.ToByte(key);

        if (keyByte == ToggleKey && !ImGuiRenderer.Failed)
            return false;

        if (ImGuiRenderer.OverlayVisible)
            EnqueueKey(args, key, down: false);

        if (_gameHeld.Remove(keyByte))
            return true;

        return !ImGuiRenderer.OverlayVisible;
    }

    // Queues the key/button for ImGui. Returns true if it is a mouse button.
    private static bool EnqueueKey(object args, object key, bool down)
    {
        if (KeyMap.IsMouseKey(key))
        {
            var btnIdx = KeyMap.MouseKeyToImGuiButton(key);
            if (btnIdx >= 0)
                _ioQueue.Enqueue(io => io.AddMouseButtonEvent(btnIdx, down));
            return true;
        }

        // Kept as int: a closure field of ImGui.NET's enum type would make this compiler-generated
        // class need ImGui.NET at type-load time (Assembly.GetTypes() before our resolver exists).
        var imKey = (int)KeyMap.ToImGuiKey(key);
        var ctrl  = Convert.ToBoolean(Read(args, "Control") ?? false);
        var shift = Convert.ToBoolean(Read(args, "Shift") ?? false);
        var alt   = Convert.ToBoolean(Read(args, "Alt") ?? false);
        var super = Convert.ToBoolean(Read(args, "System") ?? false);

        _ioQueue.Enqueue(io =>
        {
            io.AddKeyEvent(ImGuiKey.ModCtrl, ctrl);
            io.AddKeyEvent(ImGuiKey.ModShift, shift);
            io.AddKeyEvent(ImGuiKey.ModAlt, alt);
            io.AddKeyEvent(ImGuiKey.ModSuper, super);
            if (imKey != (int)ImGuiKey.None)
                io.AddKeyEvent((ImGuiKey)imKey, down);
        });
        return false;
    }

    private static bool OnMouseMove(object args)
    {
        if (!ImGuiRenderer.OverlayVisible) return true;

        var screenCoords = Read(args, "Position");
        Vector2 pos = default;

        if (screenCoords is not null)
        {
            var innerPos = Read(screenCoords, "Position");
            if (innerPos is Vector2 v) pos = v;
        }

        var virtualPos = pos / UiScale.K;
        _ioQueue.Enqueue(io => io.AddMousePosEvent(virtualPos.X, virtualPos.Y));
        return !WantCaptureMouse;
    }

    private static bool OnMouseWheel(object args)
    {
        if (!ImGuiRenderer.OverlayVisible) return true;

        var delta = Read(args, "Delta");
        Vector2 d = default;
        if (delta is Vector2 v) d = v;

        _ioQueue.Enqueue(io => io.AddMouseWheelEvent(d.X, d.Y));

        return !WantCaptureMouse;
    }

    private static bool OnTextEntered(object args)
    {
        if (!ImGuiRenderer.OverlayVisible) return true;

        var textObj = Read(args, "Text");

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

    private static object? Read(object obj, string name)
        => ReadProperty(obj, name) ?? ReadField(obj, name);

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
public static class ImGuiSdl3TextInputStopPatch
{
    private static bool _loggedBlock;

    private static MethodBase? _target;

    // Harmony throws on a null TargetMethod(); decline the patch instead (e.g. GLFW backend).
    static bool Prepare()
    {
        var t = AccessTools.TypeByName("Robust.Client.Graphics.Clyde.Clyde+Sdl3WindowingImpl");
        _target = t is null ? null
            : AccessTools.Method(t, "TextInputStop") ?? AccessTools.Method(t, "StopTextInput");

        MarseyLogger.Info($"[ImGuiSdl3TextInputStopPatch] Target: {_target?.FullDescription() ?? "NOT FOUND — skipped"}");
        return _target is not null;
    }

    [HarmonyTargetMethod]
    private static MethodBase TargetMethod() => _target!;

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