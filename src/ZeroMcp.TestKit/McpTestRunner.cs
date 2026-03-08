using System.Diagnostics;
using System.Text.Json;
using ZeroMcp.TestKit.Models;

namespace ZeroMcp.TestKit;

/// <summary>
/// Invokes the mcptest engine binary as a child process and parses the JSON result.
/// </summary>
public class McpTestRunner
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        WriteIndented = true
    };

    private readonly string _enginePath;

    public McpTestRunner() : this(EngineResolver.Resolve()) { }

    public McpTestRunner(string enginePath)
    {
        _enginePath = enginePath;
    }

    /// <summary>
    /// Execute a test definition against the mcptest engine.
    /// </summary>
    /// <param name="definition">The test definition to run.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The structured test result.</returns>
    public async Task<McpTestRunResult> RunAsync(
        McpTestDefinition definition,
        CancellationToken cancellationToken = default)
    {
        var definitionPath = Path.GetTempFileName();
        var resultPath = Path.Combine(Path.GetTempPath(), $"mcptest-result-{Guid.NewGuid():N}.json");

        try
        {
            var json = JsonSerializer.Serialize(definition, JsonOptions);
            await File.WriteAllTextAsync(definitionPath, json, cancellationToken);

            var args = $"run --file \"{definitionPath}\" --output \"{resultPath}\"";

            var psi = new ProcessStartInfo
            {
                FileName = _enginePath,
                Arguments = args,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
            };

            using var process = new Process { StartInfo = psi };
            var stderr = new List<string>();

            process.ErrorDataReceived += (_, e) =>
            {
                if (e.Data is not null)
                    stderr.Add(e.Data);
            };

            process.Start();
            process.BeginErrorReadLine();

            var stdout = await process.StandardOutput.ReadToEndAsync(cancellationToken);
            await process.WaitForExitAsync(cancellationToken);

            if (!File.Exists(resultPath))
            {
                if (stdout.Length > 0)
                {
                    return DeserializeResult(stdout);
                }

                throw new McpTestException(
                    $"mcptest exited with code {process.ExitCode} and produced no result file.\n" +
                    $"stderr: {string.Join(Environment.NewLine, stderr)}");
            }

            var resultJson = await File.ReadAllTextAsync(resultPath, cancellationToken);
            return DeserializeResult(resultJson);
        }
        finally
        {
            TryDelete(definitionPath);
            TryDelete(resultPath);
        }
    }

    private static McpTestRunResult DeserializeResult(string json)
    {
        return JsonSerializer.Deserialize<McpTestRunResult>(json, JsonOptions)
            ?? throw new McpTestException("Failed to deserialize mcptest result — null response");
    }

    private static void TryDelete(string path)
    {
        try { File.Delete(path); }
        catch { /* best effort cleanup */ }
    }
}
