using Xunit;
using ZeroMcp.TestKit.Models;

namespace ZeroMcp.TestKit.Xunit;

/// <summary>
/// Fluent extension methods for chaining assertions on MCP test results.
/// </summary>
public static class McpFluentAssertions
{
    public static McpTestRunResult HasToolName(this McpTestRunResult test, string toolName)
    {
        McpAssert.ToolPassed(test, toolName);
        return test;
    }

    public static McpTestRunResult HasValidSchema(this McpTestRunResult test, string toolName)
    {
        McpAssert.SchemaValid(test, toolName);
        return test;
    }

    public static McpTestRunResult IsDeterministic(this McpTestRunResult test, string toolName)
    {
        McpAssert.Deterministic(test, toolName);
        return test;
    }

    /// <summary>
    /// Assert that the first tool result's response contains the given property path.
    /// For multi-tool results, use <see cref="HasReturnProperty(McpTestRunResult, string, string)"/>.
    /// </summary>
    public static McpTestRunResult HasReturnProperty(this McpTestRunResult test, string property)
    {
        var firstTool = test.Results.FirstOrDefault();
        Assert.NotNull(firstTool);
        McpAssert.ResponseHasProperty(test, firstTool!.Tool, property);
        return test;
    }

    /// <summary>
    /// Assert that the specified tool's response contains the given property path.
    /// </summary>
    public static McpTestRunResult HasReturnProperty(this McpTestRunResult test, string toolName, string property)
    {
        McpAssert.ResponseHasProperty(test, toolName, property);
        return test;
    }

    /// <summary>
    /// Assert that the specified tool's response contains the given property with the expected value.
    /// </summary>
    public static McpTestRunResult HasReturnValue(this McpTestRunResult test, string toolName, string propertyPath, string expectedValue)
    {
        McpAssert.ResponseContains(test, toolName, propertyPath, expectedValue);
        return test;
    }

    public static McpTestRunResult Passed(this McpTestRunResult test)
    {
        McpAssert.Passed(test);
        return test;
    }

    public static McpTestRunResult Failed(this McpTestRunResult test)
    {
        if (test.Passed)
            Assert.Fail("Expected test to fail, but it passed.");
        return test;
    }
}
