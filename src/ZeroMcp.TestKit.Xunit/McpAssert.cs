using Xunit;
using ZeroMcp.TestKit.Models;

namespace ZeroMcp.TestKit.Xunit;

/// <summary>
/// xUnit assertion helpers for MCP test results.
/// </summary>
public static class McpAssert
{
    /// <summary>
    /// Assert that the test run passed. Fails with detailed error messages if not.
    /// </summary>
    public static void Passed(McpTestRunResult result)
    {
        if (result.Passed)
            return;

        var failures = result.Results.Where(r => !r.Passed).ToList();
        var messages = failures.SelectMany(f =>
            f.Errors.Select(e => $"[{f.Tool}] [{e.Category}] {e.Message}"));

        Assert.Fail(
            $"MCP test run failed ({failures.Count} of {result.Results.Count} tests):\n" +
            string.Join("\n", messages));
    }

    /// <summary>
    /// Assert that a specific tool's test passed.
    /// </summary>
    public static void ToolPassed(McpTestRunResult result, string toolName)
    {
        var toolResult = result.Results.Find(r => r.Tool == toolName);
        Assert.NotNull(toolResult);

        if (toolResult!.Passed)
            return;

        var messages = toolResult.Errors.Select(e => $"[{e.Category}] {e.Message}");
        Assert.Fail($"Tool '{toolName}' failed:\n" + string.Join("\n", messages));
    }

    /// <summary>
    /// Assert that a specific tool's schema validation passed.
    /// </summary>
    public static void SchemaValid(McpTestRunResult result, string toolName)
    {
        var toolResult = result.Results.Find(r => r.Tool == toolName);
        Assert.NotNull(toolResult);
        Assert.True(toolResult!.SchemaValid, $"Tool '{toolName}' schema validation failed");
    }

    /// <summary>
    /// Assert that a specific tool's determinism check passed.
    /// </summary>
    public static void Deterministic(McpTestRunResult result, string toolName)
    {
        var toolResult = result.Results.Find(r => r.Tool == toolName);
        Assert.NotNull(toolResult);
        Assert.True(toolResult!.Deterministic, $"Tool '{toolName}' is not deterministic");
    }
}
