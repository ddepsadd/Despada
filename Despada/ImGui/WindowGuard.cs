// SPDX-FileCopyrightText: 2026 ddepsadd <https://github.com/ddepsadd>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Numerics;
using System.Runtime.InteropServices;
using ImGuiNET;

namespace Despada.ImGui;

// Isolates UI code per window: an exception in one window turns it into an error stub
// instead of killing the whole overlay. ImGui stacks (Begin/Push*/ID) left open by the
// failed window are unwound through ImGui's error recovery API.
internal static unsafe class WindowGuard
{
    private static delegate* unmanaged<nint, void> _storeState;
    private static delegate* unmanaged<nint, void> _recoverState;
    private static nint _state;

    private static readonly Dictionary<string, Exception> _faulted = new();

    public static void Initialize()
    {
        var lib = NativeLoader.Handle;
        if (lib == 0
            || !NativeLibrary.TryGetExport(lib, "ImGuiErrorRecoveryState_ImGuiErrorRecoveryState", out var ctor)
            || !NativeLibrary.TryGetExport(lib, "igErrorRecoveryStoreState", out var store)
            || !NativeLibrary.TryGetExport(lib, "igErrorRecoveryTryToRecoverState", out var recover))
        {
            MarseyLogger.Warn("[WindowGuard] Error recovery exports not found — relying on end-of-frame recovery only.");
            return;
        }

        _state        = ((delegate* unmanaged<nint>)ctor)();
        _storeState   = (delegate* unmanaged<nint, void>)store;
        _recoverState = (delegate* unmanaged<nint, void>)recover;
    }

    public static void Draw(string id, Action draw)
    {
        if (_faulted.TryGetValue(id, out var error))
        {
            DrawFaultStub(id, error);
            return;
        }

        if (_state != 0) _storeState(_state);
        try
        {
            draw();
        }
        catch (Exception ex)
        {
            if (_state != 0) _recoverState(_state);
            _faulted[id] = ex;
            MarseyLogger.Fatal($"[WindowGuard] '{id}' failed and was disabled: {ex}");
        }
    }

    private static void DrawFaultStub(string id, Exception error)
    {
        ImGuiNET.ImGui.SetNextWindowSize(new Vector2(420f, 0f), ImGuiCond.Appearing);
        if (ImGuiNET.ImGui.Begin($"{id} — error##fault_{id}", ImGuiWindowFlags.NoSavedSettings | ImGuiWindowFlags.AlwaysAutoResize))
        {
            ImGuiNET.ImGui.PushStyleColor(ImGuiCol.Text, Theme.Hex(0xFF6B6B));
            ImGuiNET.ImGui.TextUnformatted($"{error.GetType().Name}");
            ImGuiNET.ImGui.PopStyleColor();

            ImGuiNET.ImGui.PushTextWrapPos(400f);
            ImGuiNET.ImGui.TextUnformatted(error.Message);
            ImGuiNET.ImGui.PopTextWrapPos();

            ImGuiNET.ImGui.Spacing();
            if (ImGuiNET.ImGui.Button("Retry"))
                _faulted.Remove(id);
        }
        ImGuiNET.ImGui.End();
    }
}
