using System.Reflection;
using System.Runtime.InteropServices;

namespace Despada.ImGui;
internal static class NativeLoader
{
    private static string? _tempDir;
    private static bool    _loaded;

    public static void EnsureLoaded()
    {
        if (_loaded) return;
        _loaded = true;

        _tempDir = Path.Combine(Path.GetTempPath(), $"despada_{Environment.ProcessId}");
        Directory.CreateDirectory(_tempDir);
        MarseyLogger.Info($"[NativeLoader] Temp dir: {_tempDir}");

        var asm    = typeof(NativeLoader).Assembly;
        var soPath = ExtractResource(asm, "Native.libcimgui.so", "libcimgui.so");
        if (soPath is null) return;

        try
        {
            var handle = NativeLibrary.Load(soPath);
            MarseyLogger.Info($"[NativeLoader] Preloaded libcimgui.so, handle=0x{handle:X}");
        }
        catch (Exception ex)
        {
            MarseyLogger.Fatal($"[NativeLoader] Preload failed: {ex.Message}");
            return;
        }
        TryRegisterForAssembly(typeof(ImGuiNET.ImGui).Assembly, _tempDir);
    }

    private static string? ExtractResource(Assembly asm, string resourceSuffix, string fileName)
    {
        var resourceName = $"{asm.GetName().Name}.{resourceSuffix}";
        var destPath     = Path.Combine(_tempDir!, fileName);

        using var stream = asm.GetManifestResourceStream(resourceName);
        if (stream is null)
        {
            MarseyLogger.Fatal($"[NativeLoader] Resource not found: '{resourceName}'");
            MarseyLogger.Fatal($"[NativeLoader] Available: {string.Join(", ", asm.GetManifestResourceNames())}");
            return null;
        }

        using (var file = File.Create(destPath))
            stream.CopyTo(file);

        MarseyLogger.Info($"[NativeLoader] Extracted → {destPath}");
        return destPath;
    }

    private static void TryRegisterForAssembly(Assembly asm, string dir)
    {
        try
        {
            NativeLibrary.SetDllImportResolver(asm, (libraryName, assembly, searchPath) =>
            {
                var candidates = new[]
                {
                    libraryName,
                    libraryName + ".so",
                    "lib" + libraryName + ".so"
                };

                foreach (var candidate in candidates)
                {
                    var path = Path.Combine(dir, candidate);
                    if (File.Exists(path) && NativeLibrary.TryLoad(path, out var h))
                    {
                        MarseyLogger.Info($"[NativeLoader] Resolved '{libraryName}' → {path}");
                        return h;
                    }
                }

                NativeLibrary.TryLoad(libraryName, assembly, searchPath, out var fallback);
                return fallback;
            });

            MarseyLogger.Info($"[NativeLoader] Resolver registered for {asm.GetName().Name}");
        }
        catch (InvalidOperationException) {}
    }

    public static void Cleanup()
    {
        if (_tempDir is null || !Directory.Exists(_tempDir)) return;
        try { Directory.Delete(_tempDir, recursive: true); }
        catch {}
    }
}