// SPDX-FileCopyrightText: 2026 ddepsadd <https://github.com/ddepsadd>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Reflection;
using HarmonyLib;

public static class MarseyEntry
{
    public static void Entry()
    {
        MarseyLogger.Info("Entry for patching started.");

        // Entry runs from MarseyLoader's postfix on ModLoader.TryLoadModules, so content is
        // already loaded by now; there's nothing to wait for.
        if (FindAssembly("Content.Client") is null)
        {
            MarseyLogger.Fatal("Content.Client is not loaded — Despada disabled.");
            return;
        }

        PatchAll(Assembly.GetExecutingAssembly());

        Sedition.Hide();
        Sedition.Apply(new Sedition.Manifest
        {
            StackNames = { "Despada" },
        });
    }

    // Same as Harmony.PatchAll, but one failing patch class no longer aborts the rest
    // (and no exception escapes into the engine's module loading).
    private static void PatchAll(Assembly asm)
    {
        foreach (var type in AccessTools.GetTypesFromAssembly(asm))
        {
            try
            {
                SubverterPatch.Harm.CreateClassProcessor(type).Patch();
            }
            catch (Exception ex)
            {
                MarseyLogger.Fatal($"Patching {type.FullName} failed, feature disabled: {ex}");
            }
        }
    }

    private static Assembly? FindAssembly(string assemblyName)
    {
        var asmList = AppDomain.CurrentDomain.GetAssemblies();
        return asmList.FirstOrDefault(asm => asm.GetName().Name == assemblyName);
    }
}