using System.Runtime.InteropServices;

namespace Despada.ImGui;

/// <summary>
/// Cross-platform OpenGL entry-point resolver.
///
/// Loading GL functions by DllImport("libGL.so.1") only works on Linux. On Windows,
/// opengl32.dll exports just OpenGL 1.1 — everything modern (shaders, VAOs, glActiveTexture,
/// glBlendFuncSeparate, …) must come from wglGetProcAddress. This resolves addresses the way
/// the platform actually wants, then callers wrap them in delegates.
///
/// Must be called while a GL context is current (wglGetProcAddress requires it). All of our
/// use happens inside Clyde.Render(), where the main context is current, so that holds.
/// x64 only — the native calling convention is unified there, so delegate cc doesn't matter.
/// </summary>
internal static class GlLoader
{
    private static readonly bool _isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
    private static readonly bool _isMac     = RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

    private static nint _glModule;
    private static bool _moduleTried;

    [DllImport("opengl32.dll", EntryPoint = "wglGetProcAddress", CharSet = CharSet.Ansi)]
    private static extern nint WglGetProcAddress(string name);

    private static void EnsureModule()
    {
        if (_moduleTried) return;
        _moduleTried = true;

        var lib = _isWindows ? "opengl32.dll"
                : _isMac     ? "/System/Library/Frameworks/OpenGL.framework/OpenGL"
                :              "libGL.so.1";

        if (!NativeLibrary.TryLoad(lib, out _glModule))
            MarseyLogger.Fatal($"[GlLoader] Failed to load GL library '{lib}'");
    }

    public static nint GetProc(string name)
    {
        EnsureModule();

        if (_isWindows)
        {
            var p = WglGetProcAddress(name);
            // wglGetProcAddress returns 0/1/2/3/-1 for functions it won't provide (core GL 1.1);
            // those live in opengl32.dll itself.
            if (p is 0 or 1 or 2 or 3 or -1)
                return _glModule != 0 && NativeLibrary.TryGetExport(_glModule, name, out var e) ? e : 0;
            return p;
        }

        // Linux/macOS: the GL library exports every function symbol directly.
        return _glModule != 0 && NativeLibrary.TryGetExport(_glModule, name, out var ex) ? ex : 0;
    }

    /// <summary>Resolve <paramref name="name"/> and marshal it into delegate <typeparamref name="T"/>.</summary>
    public static T Load<T>(string name) where T : Delegate
    {
        var p = GetProc(name);
        if (p == 0)
        {
            MarseyLogger.Fatal($"[GlLoader] Could not resolve GL function '{name}'");
            return null!;
        }
        return Marshal.GetDelegateForFunctionPointer<T>(p);
    }
}
