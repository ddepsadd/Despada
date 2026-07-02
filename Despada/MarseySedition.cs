using System;
using System.Collections.Generic;
using System.Reflection;

/// <summary>
/// Patch-side Hidesey bridge. Lives in the subverter patch (Despada) and does NOT reference
/// Marsey — the loader finds this class by name at load time and fills the delegate fields by
/// reflection (see Marsey.Subversion.Sedition.SetupSedition). The patch then calls these methods
/// blindly; if the loader hasn't bound a delegate (older loader), the call is a safe no-op.
///
/// Field names MUST stay in sync with the loader's BindDelegate calls:
///   hideDelegate          ← Marsey...Sedition.HideDelegate
///   hideManifestDelegate  ← Marsey...Sedition.HideManifestDelegate
/// </summary>
public static class Sedition
{
    /// <summary>Hides the calling assembly from the anticheat (reflection/VFS/stack channels).</summary>
    public static Action<Assembly>? hideDelegate;

    /// <summary>
    /// Raw manifest bridge. Order: (systems, comps, ioc, cvars, commands, stackNames).
    /// Primitives only — the real HideManifest type lives in Marsey, which this assembly can't see.
    /// </summary>
    public static Action<string[], string[], string[], string[], string[], string[]>? hideManifestDelegate;

    /// <summary>Hide this assembly.</summary>
    public static void Hide() => hideDelegate?.Invoke(Assembly.GetExecutingAssembly());

    /// <summary>
    /// Local mirror of HideManifest — declarative description of what to hide from the anticheat.
    /// All entries are STRINGS (FullName for type categories, registered name for cvars/commands)
    /// so they can be declared from Entry before the targets are loaded.
    ///
    ///   Systems    — EntitySystem FullName
    ///   Comps      — Component FullName  (a [NetworkedComponent] will be refused loader-side)
    ///   Ioc        — service/impl FullName
    ///   Cvars      — cvar name
    ///   Commands   — command name (removed from AvailableCommands enumeration)
    ///   StackNames — substring to scrub from stack traces (usually unnecessary — hidden
    ///                assemblies are scrubbed automatically)
    /// </summary>
    public sealed class Manifest
    {
        public List<string> Systems    { get; } = new();
        public List<string> Comps      { get; } = new();
        public List<string> Ioc        { get; } = new();
        public List<string> Cvars      { get; } = new();
        public List<string> Commands   { get; } = new();
        public List<string> StackNames { get; } = new();
    }

    /// <summary>Apply a manifest. Routed to the loader, which builds the real HideManifest.</summary>
    public static void Apply(Manifest m)
    {
        if (m == null) return;
        hideManifestDelegate?.Invoke(
            m.Systems.ToArray(),
            m.Comps.ToArray(),
            m.Ioc.ToArray(),
            m.Cvars.ToArray(),
            m.Commands.ToArray(),
            m.StackNames.ToArray());
    }

    // ── thin one-off wrappers (grep-friendly) ──
    public static void HideSystem(string fullName)  { var m = new Manifest(); m.Systems.Add(fullName);  Apply(m); }
    public static void HideComp(string fullName)    { var m = new Manifest(); m.Comps.Add(fullName);    Apply(m); }
    public static void HideIoc(string fullName)     { var m = new Manifest(); m.Ioc.Add(fullName);      Apply(m); }
    public static void HideCvar(string name)        { var m = new Manifest(); m.Cvars.Add(name);        Apply(m); }
    public static void HideCommand(string name)     { var m = new Manifest(); m.Commands.Add(name);     Apply(m); }
}