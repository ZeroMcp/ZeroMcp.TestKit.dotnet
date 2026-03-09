using Xunit;
using ZeroMcp.TestKit.Models;

namespace ZeroMcp.TestKit.Xunit;

/// <summary>
/// Fluent extension methods for chaining assertions on MCP test results.
/// </summary>
public static class McpFluentAssertions
{
    public static McpToolTestResult ForTool(this McpTestRunResult test, string toolName)
    {
        var toolResult = test.Results.FirstOrDefault(r => r.Tool == toolName);
        Assert.NotNull(toolResult);
        return toolResult!;
    }
    public static McpToolTestResult HasToolName(this McpToolTestResult test, string toolName)
    {
        Assert.Equal(test.Tool, toolName);
        return test;
    }

    public static McpToolTestResult HasValidSchema(this McpToolTestResult test)
    {
        Assert.True(test.SchemaValid);
        return test;
    }

    public static McpToolTestResult IsDeterministic(this McpToolTestResult test)
    {
        Assert.True(test.Deterministic);
        return test;
    }

    /// <summary>
    /// Assert that the first tool result's response contains the given property path.
    /// For multi-tool results, use <see cref="HasReturnProperty(McpTestRunResult, string, string)"/>.
    /// </summary>
    public static McpToolTestResult HasReturnProperty(this McpToolTestResult test, string property)
    {
        Assert.True(test!.Response.HasValue, $"Tool '{test.Tool}' has no response");

        var payload = McpAssert.ExtractPayload(test.Response!.Value);
        try
        {
            McpAssert.NavigatePath(payload, property);
        }
        catch (Exception ex)
        {
            Assert.Fail($"Tool '{test.Tool}' response does not contain path '{property}': {ex.Message}");
        }
        return test;
    }

    /// <summary>
    /// Assert that the specified tool's response contains the given property with the expected value.
    /// </summary>
    public static McpToolTestResult HasReturnValue(this McpToolTestResult toolResult, string propertyPath, string expectedValue)
    {
        Assert.NotNull(toolResult);
        Assert.True(toolResult!.Response.HasValue, $"Tool '{toolResult.Tool}' has no response");

        var payload = McpAssert.ExtractPayload(toolResult.Response!.Value);
        var element = McpAssert.NavigatePath(payload, propertyPath);
        var actual = element.ToString();
        Assert.Equal(expectedValue, actual);
        return toolResult;
    }

    public static McpToolTestResult Passed(this McpToolTestResult test)
    {
        Assert.True(test.Passed);
        return test;
    }

    public static McpToolTestResult Failed(this McpToolTestResult test)
    {
        Assert.False(test.Passed);
        return test;
    }
}
