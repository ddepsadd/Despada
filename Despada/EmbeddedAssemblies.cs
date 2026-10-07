// SPDX-FileCopyrightText: 2026 ddepsadd <https://github.com/ddepsadd>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Reflection;
using System.Runtime.CompilerServices;

// Managed dependencies (ImGui.NET, ...) ship inside Despada.dll as "Despada.Deps.<Name>.dll"
// resources, so the mod is a single file. They are loaded on demand when the runtime fails
// to find them next to Despada.dll.
internal static class EmbeddedAssemblies
{
    private const string Prefix = "Despada.Deps.";

    private static readonly Dictionary<string, Assembly> _loaded = new();
    private static bool _installed;

    // Runs before any Despada code executes, i.e. before anything can touch an ImGui type.
#pragma warning disable CA2255 // A loaded mod has no application entry point to do this from.
    [ModuleInitializer]
#pragma warning restore CA2255
    internal static void Install()
    {
        if (_installed) return;
        _installed = true;
        AppDomain.CurrentDomain.AssemblyResolve += Resolve;
    }

    private static Assembly? Resolve(object? sender, ResolveEventArgs args)
    {
        var name = new AssemblyName(args.Name).Name;
        if (name is null) return null;

        lock (_loaded)
        {
            if (_loaded.TryGetValue(name, out var cached))
                return cached;

            using var stream = typeof(EmbeddedAssemblies).Assembly.GetManifestResourceStream($"{Prefix}{name}.dll");
            if (stream is null) return null;

            var bytes = new byte[stream.Length];
            stream.ReadExactly(bytes);

            var asm = Assembly.Load(bytes);
            _loaded[name] = asm;
            MarseyLogger.Info($"[EmbeddedAssemblies] Loaded {asm.FullName} from resources.");
            return asm;
        }
    }
}
