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
    /// Assert that a tool's response contains a property at the given JSON path with the expected value.
    /// Path uses dot notation: "content[0].text" or simple property names: "content".
    /// </summary>
    public static void ResponseContains(McpTestRunResult result, string toolName, string propertyPath, string expectedValue)
    {
        var toolResult = result.Results.Find(r => r.Tool == toolName);
        Assert.NotNull(toolResult);
        Assert.True(toolResult!.Response.HasValue, $"Tool '{toolName}' has no response");

        var element = NavigatePath(toolResult.Response!.Value, propertyPath);
        var actual = element.ToString();
        Assert.Equal(expectedValue, actual);
    }

    /// <summary>
    /// Assert that a tool's response contains a property at the given JSON path.
    /// </summary>
    public static void ResponseHasProperty(McpTestRunResult result, string toolName, string propertyPath)
    {
        var toolResult = result.Results.Find(r => r.Tool == toolName);
        Assert.NotNull(toolResult);
        Assert.True(toolResult!.Response.HasValue, $"Tool '{toolName}' has no response");

        try
        {
            NavigatePath(toolResult.Response!.Value, propertyPath);
        }
        catch (Exception ex)
        {
            Assert.Fail($"Tool '{toolName}' response does not contain path '{propertyPath}': {ex.Message}");
        }
    }

    /// <summary>
    /// Get the raw response JsonElement for a specific tool (for custom assertions).
    /// </summary>
    public static JsonElement GetResponse(McpTestRunResult result, string toolName)
    {
        var toolResult = result.Results.Find(r => r.Tool == toolName);
        Assert.NotNull(toolResult);
        Assert.True(toolResult!.Response.HasValue, $"Tool '{toolName}' has no response");
        return toolResult.Response!.Value;
    }

    private static JsonElement NavigatePath(JsonElement root, string path)
    {
        var current = root;
        var segments = path.Split('.');

        foreach (var segment in segments)
        {
            var bracketIdx = segment.IndexOf('[');
            if (bracketIdx >= 0)
            {
                var propName = segment[..bracketIdx];
                var indexStr = segment[(bracketIdx + 1)..^1];
                var index = int.Parse(indexStr);

                if (propName.Length > 0)
                    current = current.GetProperty(propName);

                current = current[index];
            }
            else
            {
                current = current.GetProperty(segment);
            }
        }

        return current;
    }
}
