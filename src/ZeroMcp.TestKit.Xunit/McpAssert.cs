using System.Text.Json;
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

    /// <summary>
    /// Assert that at least one tool response contains the specified property.
    /// </summary>
    public static void HasProperty(McpTestRunResult result, string property)
    {
        Assert.NotEmpty(result.Results);

        foreach (var toolResult in result.Results)
        {
            if (toolResult.Response is { } response
                && response.ValueKind == JsonValueKind.Object
                && response.TryGetProperty(property, out _))
            {
                return;
            }
        }

        var toolNames = string.Join(", ", result.Results.Select(r => r.Tool));
        Assert.Fail(
            $"No tool response contains property '{property}'. " +
            $"Tools checked: [{toolNames}]");
    }

    /// <summary>
    /// Assert that a specific tool's response contains the specified property.
    /// </summary>
    public static void HasProperty(McpTestRunResult result, string toolName, string property)
    {
        var toolResult = result.Results.Find(r => r.Tool == toolName);
        Assert.NotNull(toolResult);

        Assert.True(
            toolResult!.Response is { } response
            && response.ValueKind == JsonValueKind.Object
            && response.TryGetProperty(property, out _),
            $"Tool '{toolName}' response does not contain property '{property}'");
    }
}
