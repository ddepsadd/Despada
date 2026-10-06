// SPDX-FileCopyrightText: 2026 ddepsadd <https://github.com/ddepsadd>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Numerics;
using System.Runtime.InteropServices;
using Despada.ImGui;
using ImGuiNET;

namespace Despada.ImGui.UserInterface;

public static partial class Widgets
{
    private static int _sectionId;

    private static readonly Vector4 SectionBg      = Theme.Hex(0x141418);
    private static readonly uint    SectionBorder  = Theme.ToU32(Theme.HexA(0xFFFFFF, 0.08f));
    private static readonly uint    SeparatorColor = Theme.ToU32(Theme.HexA(0xFFFFFF, 0.08f));

    private const float SectionPad    = 12f;
    private const float SectionGap    = 14f;
    private const float SectionRound  = 10f;
    private const float ContentIndent = 16f;
    private const float HeaderPad     = 16f;
    private const float HeaderH       = 64f;

    private const int StateSweepEvery = 120;
    private const int StateTtlFrames  = 600;

    public static void ResetFrame()
    {
        _sectionId = 0;

        var frame = ImGuiNET.ImGui.GetFrameCount();
        if (frame % StateSweepEvery != 0) return;

        _anims.Sweep(frame, StateTtlFrames);
        _comboPage.Sweep(frame, StateTtlFrames);
        _sliderKnobX.Sweep(frame, StateTtlFrames);
        _colorPickerState.Sweep(frame, StateTtlFrames);
    }

    /// <summary>Call once per frame after all widgets — advances and draws shared effects.</summary>
    public static void EndFrame()
    {
        UpdateSparks(ImGuiNET.ImGui.GetForegroundDrawList(), ImGuiNET.ImGui.GetIO().DeltaTime);
    }

    /// <summary>
    /// Per-widget state keyed by ImGui ID. Entries not touched for a while are dropped by
    /// <see cref="Sweep"/>, so widgets that stop being drawn don't leak.
    /// </summary>
    private sealed class StateStore<T>
    {
        private struct Entry
        {
            public T   Value;
            public int Frame;
        }

        private readonly Dictionary<uint, Entry> _map = new();

        /// <summary>The returned ref is only valid until the next Get on this store.</summary>
        public ref T Get(uint id, T initial)
        {
            ref var e = ref CollectionsMarshal.GetValueRefOrAddDefault(_map, id, out var exists);
            if (!exists) e.Value = initial;
            e.Frame = ImGuiNET.ImGui.GetFrameCount();
            return ref e.Value;
        }

        public void Sweep(int frame, int ttl)
        {
            foreach (var (id, e) in _map)
                if (frame - e.Frame > ttl)
                    _map.Remove(id);
        }
    }

    private struct AnimState
    {
        public float T;
        public int   SteppedFrame;
    }

    private static readonly StateStore<AnimState> _anims = new();

    /// <summary>0→1 eased towards <paramref name="value"/>; stepped at most once per frame per id.</summary>
    private static float Animate(uint id, bool value, float speed = ToggleSpeed)
    {
        var target = value ? 1f : 0f;
        ref var a = ref _anims.Get(id, new AnimState { T = target, SteppedFrame = -1 });

        var frame = ImGuiNET.ImGui.GetFrameCount();
        if (a.SteppedFrame != frame)
        {
            a.SteppedFrame = frame;
            a.T = Anim.Lerp(a.T, target, speed, ImGuiNET.ImGui.GetIO().DeltaTime);
        }

        return a.T;
    }

    /// <summary>Invisible button at an absolute screen position. Cursor is left after the item.</summary>
    private static bool ButtonAt(string id, Vector2 min, Vector2 size, out bool hovered)
    {
        ImGuiNET.ImGui.SetCursorScreenPos(min);
        var pressed = ImGuiNET.ImGui.InvisibleButton(id, size);
        hovered = ImGuiNET.ImGui.IsItemHovered();
        return pressed;
    }

    private static string DisplayLabel(string label)
    {
        var idx = label.IndexOf("##", StringComparison.Ordinal);
        return idx >= 0 ? label[..idx] : label;
    }
}
