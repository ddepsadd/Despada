// SPDX-FileCopyrightText: 2026 ddepsadd <https://github.com/ddepsadd>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Reflection;

/// <summary>
/// Patch-side logging bridge. The loader finds this class by name and binds
/// <see cref="logDelegate"/> to its own MarseyLogger::Log(AssemblyName, string)
/// via reflection (Marsey.PatchAssembly.AssemblyFieldHandler.SetupLogger).
///
/// Loader contract — do not rename:
///   type name  : MarseyLogger
///   field name : logDelegate (public static)
///   field type : delegate compatible with void (AssemblyName, string)
///
/// If the loader never binds the field, every call is a no-op.
/// </summary>
public static class MarseyLogger
{
    public delegate void Forward(AssemblyName asm, string message);

    public static Forward? logDelegate;

    private static void Emit(string level, string message)
        => logDelegate?.Invoke(Assembly.GetExecutingAssembly().GetName(), $"[{level}] {message}");

    public static void Info(string message) => Emit("INFO", message);
    public static void Warn(string message) => Emit("WARN", message);
    public static void Fatal(string message) => Emit("FATL", message);
    public static void Debug(string message) => Emit("DEBG", message);
}