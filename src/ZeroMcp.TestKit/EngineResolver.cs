using System.Runtime.InteropServices;

namespace ZeroMcp.TestKit;

/// <summary>
/// Locates the mcptest engine binary at runtime.
/// Resolution order: MCPTEST_PATH env var, NuGet native assets, PATH.
/// </summary>
public static class EngineResolver
{
    private const string EnvVar = "MCPTEST_PATH";
    private static readonly string BinaryName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
        ? "mcptest.exe"
        : "mcptest";

    /// <summary>
    /// Resolve the full path to the mcptest binary.
    /// </summary>
    /// <returns>Absolute path to the mcptest executable.</returns>
    /// <exception cref="McpTestException">Thrown when the binary cannot be found.</exception>
    public static string Resolve()
    {
        // 1. Explicit environment variable
        var envPath = Environment.GetEnvironmentVariable(EnvVar);
        if (!string.IsNullOrWhiteSpace(envPath) && File.Exists(envPath))
            return Path.GetFullPath(envPath);

        // 2. NuGet native assets (runtimes/{rid}/native/)
        var rid = RuntimeInformation.RuntimeIdentifier;
        var baseDir = AppContext.BaseDirectory;
        var nativePath = Path.Combine(baseDir, "runtimes", rid, "native", BinaryName);
        if (File.Exists(nativePath))
            return Path.GetFullPath(nativePath);

        // 3. Fallback RID patterns (e.g. win-x64 when full RID is win10-x64)
        var arch = RuntimeInformation.OSArchitecture.ToString().ToLowerInvariant();
        string fallbackRid;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            fallbackRid = $"win-{arch}";
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            fallbackRid = $"linux-{arch}";
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            fallbackRid = $"osx-{arch}";
        else
            fallbackRid = rid;

        if (fallbackRid != rid)
        {
            var fallbackPath = Path.Combine(baseDir, "runtimes", fallbackRid, "native", BinaryName);
            if (File.Exists(fallbackPath))
                return Path.GetFullPath(fallbackPath);
        }

        // 4. Search PATH
        var pathDirs = Environment.GetEnvironmentVariable("PATH")?.Split(Path.PathSeparator) ?? [];
        foreach (var dir in pathDirs)
        {
            var candidate = Path.Combine(dir, BinaryName);
            if (File.Exists(candidate))
                return Path.GetFullPath(candidate);
        }

        throw new McpTestException(
            $"Could not find the mcptest engine binary. Searched:\n" +
            $"  1. ${EnvVar} environment variable\n" +
            $"  2. NuGet native assets: {nativePath}\n" +
            $"  3. System PATH\n\n" +
            $"Set {EnvVar} to the full path of the mcptest binary, or install the ZeroMcp.TestKit NuGet package with native assets.");
    }
}
