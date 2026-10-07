// SPDX-FileCopyrightText: 2026 ddepsadd <https://github.com/ddepsadd>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace Despada.ImGui;
internal static class NativeLoader
{
    private static string? _tempDir;
    private static bool    _loaded;
    
    private static nint    _resolvedHandle;
    private static string? _resolvedName;

    // Preloaded cimgui handle, used for our own native calls (error recovery etc.).
    public static nint Handle { get; private set; }

    public static void EnsureLoaded()
    {
        if (_loaded) return;
        _loaded = true;

        var arch = RuntimeInformation.ProcessArchitecture;
        // Bundled natives: win-x64, linux-x64, universal (x64 + arm64) macOS.
        var archSupported = RuntimeInformation.IsOSPlatform(OSPlatform.OSX)
            ? arch is Architecture.X64 or Architecture.Arm64
            : arch is Architecture.X64;
        if (!archSupported)
        {
            MarseyLogger.Fatal($"[NativeLoader] No bundled cimgui for {RuntimeInformation.OSDescription} {arch}");
            return;
        }

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
            Handle = handle;
            MarseyLogger.Info($"[NativeLoader] Preloaded {fileName}, handle=0x{handle:X}");
        }
        catch (Exception ex)
        {
            MarseyLogger.Fatal($"[NativeLoader] Preload failed: {ex.Message}");
            return;
        }

        TryRegisterForAssembly(typeof(ImGuiNET.ImGui).Assembly, _tempDir!);
    }

    // Extracts into a per-user cache dir named by content hash and reuses it across launches.
    // An existing file is only trusted if its hash still matches; writes go through a temp file
    // + rename so a half-written library is never loaded.
    private static string? ExtractResource(Assembly asm, string resourceSuffix, string fileName)
    {
        var resourceName = $"{asm.GetName().Name}.{resourceSuffix}";

        byte[] data;
        using (var stream = asm.GetManifestResourceStream(resourceName))
        {
            if (stream is null)
            {
                MarseyLogger.Fatal($"[NativeLoader] Resource not found: '{resourceName}'");
                MarseyLogger.Fatal($"[NativeLoader] Available: {string.Join(", ", asm.GetManifestResourceNames())}");
                return null;
            }
            data = new byte[stream.Length];
            stream.ReadExactly(data);
        }

        var hash = Convert.ToHexString(SHA256.HashData(data));
        _tempDir = Path.Combine(CacheRoot(), hash[..16]);
        var destPath = Path.Combine(_tempDir, fileName);

        try
        {
            if (File.Exists(destPath) && Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(destPath))) == hash)
            {
                MarseyLogger.Info($"[NativeLoader] Reusing {destPath}");
                return destPath;
            }

            Directory.CreateDirectory(_tempDir);
            var tmp = Path.Combine(_tempDir, $"{fileName}.{Environment.ProcessId}.tmp");
            File.WriteAllBytes(tmp, data);
            File.Move(tmp, destPath, overwrite: true);
        }
        catch (Exception ex)
        {
            // A concurrently running client may hold the file; fall back to a private copy.
            MarseyLogger.Warn($"[NativeLoader] Cache write failed ({ex.Message}), using a per-process copy.");
            _tempDir = Path.Combine(Path.GetTempPath(), $"despada_{Environment.ProcessId}");
            Directory.CreateDirectory(_tempDir);
            destPath = Path.Combine(_tempDir, fileName);
            File.WriteAllBytes(destPath, data);
        }

        MarseyLogger.Info($"[NativeLoader] Extracted → {destPath}");
        return destPath;
    }

    private static string CacheRoot()
    {
        string root;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            root = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            root = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Library", "Caches");
        else
            root = Environment.GetEnvironmentVariable("XDG_CACHE_HOME") is { Length: > 0 } xdg
                ? xdg
                : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".cache");

        return Path.Combine(root, "Despada", "native");
    }

    private static void TryRegisterForAssembly(Assembly asm, string dir)
    {
        try
        {
            NativeLibrary.SetDllImportResolver(asm, (libraryName, assembly, searchPath) =>
            {
                if (_resolvedHandle != 0 && libraryName == _resolvedName)
                    return _resolvedHandle;

                // Hand back the preloaded image: on macOS the install name (@rpath/cimgui.dylib)
                // doesn't match the extracted file, so a fresh lookup may miss it.
                if (libraryName == "cimgui" && Handle != 0)
                    return Handle;

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

    // Only the per-process fallback copy is removed; the hashed cache is meant to be reused.
    public static void Cleanup()
    {
        if (_tempDir is null || !Path.GetFileName(_tempDir).StartsWith("despada_") || !Directory.Exists(_tempDir)) return;
        try { Directory.Delete(_tempDir, recursive: true); }
        catch {}
    }
}