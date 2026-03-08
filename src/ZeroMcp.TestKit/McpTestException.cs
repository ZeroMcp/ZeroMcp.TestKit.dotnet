using ZeroMcp.TestKit.Models;

namespace ZeroMcp.TestKit;

/// <summary>
/// Exception thrown when MCP tests fail or the engine cannot be invoked.
/// Contains structured error details from the engine when available.
/// </summary>
public class McpTestException : Exception
{
    /// <summary>
    /// The full test run result from the engine, if available.
    /// </summary>
    public McpTestRunResult? Result { get; }

    /// <summary>
    /// Individual tool failures extracted from the result for quick inspection.
    /// </summary>
    public IReadOnlyList<McpToolTestResult> Failures { get; }

    public McpTestException(string message) : base(message)
    {
        Failures = [];
    }

    public McpTestException(string message, Exception innerException) : base(message, innerException)
    {
        Failures = [];
    }

    public McpTestException(string message, McpTestRunResult result) : base(message)
    {
        Result = result;
        Failures = result.Results.Where(r => !r.Passed).ToList().AsReadOnly();
    }

    /// <summary>
    /// Build a detailed failure message from the engine result.
    /// </summary>
    public static McpTestException FromResult(McpTestRunResult result)
    {
        var failures = result.Results.Where(r => !r.Passed).ToList();
        var lines = new List<string>
        {
            $"MCP test run failed: {failures.Count} of {result.Results.Count} test(s) failed ({result.ElapsedMs}ms)"
        };

        foreach (var failure in failures)
        {
            lines.Add($"  FAIL: {failure.Tool}");
            foreach (var error in failure.Errors)
            {
                lines.Add($"    [{error.Category}] {error.Message}");
                if (error.Context is not null)
                    lines.Add($"      context: {error.Context}");
            }
        }

        return new McpTestException(string.Join(Environment.NewLine, lines), result);
    }
}
