using System.Reflection;
using System.Runtime.InteropServices;

namespace Despada.ImGui;
internal static class NativeLoader
{
    private static string? _tempDir;
    private static bool    _loaded;
    
    private static nint    _resolvedHandle;
    private static string? _resolvedName;

    public static void EnsureLoaded()
    {
        if (_loaded) return;
        _loaded = true;
        _tempDir = Path.Combine(Path.GetTempPath(), $"despada_{Environment.ProcessId}");
        Directory.CreateDirectory(_tempDir);
        MarseyLogger.Info($"[NativeLoader] Temp dir: {_tempDir}");

        var asm = typeof(NativeLoader).Assembly;

        string resourceSuffix, fileName;

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            MarseyLogger.Info("[NativeLoader] Windows detected");
            resourceSuffix = "Native.cimgui.dll";
            fileName       = "cimgui.dll";
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            MarseyLogger.Info("[NativeLoader] OSX detected");
            resourceSuffix = "Native.libcimgui.dylib";
            fileName       = "libcimgui.dylib";
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            MarseyLogger.Info("[NativeLoader] Linux detected");
            resourceSuffix = "Native.libcimgui.so";
            fileName       = "libcimgui.so";
        }
        else
        {
            MarseyLogger.Fatal("[NativeLoader] Unsupported platform");
            return;
        }

        var nativePath = ExtractResource(asm, resourceSuffix, fileName);
        if (nativePath is null) return;

        try
        {
            var handle = NativeLibrary.Load(nativePath);
            MarseyLogger.Info($"[NativeLoader] Preloaded {fileName}, handle=0x{handle:X}");
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
                if (_resolvedHandle != 0 && libraryName == _resolvedName)
                    return _resolvedHandle;

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
                        _resolvedName   = libraryName;
                        _resolvedHandle = h;
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