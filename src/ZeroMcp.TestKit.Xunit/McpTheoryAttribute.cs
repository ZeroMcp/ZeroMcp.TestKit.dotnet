using Xunit;

namespace ZeroMcp.TestKit.Xunit;

/// <summary>
/// Marks a test method as a parameterized MCP server test. Inherits from xUnit's [Theory].
/// Use with [InlineData] or [MemberData] to test multiple tool/param combinations.
/// </summary>
/// <remarks>
/// <code>
/// [McpTheory]
/// [InlineData("search", """{"query":"hello"}""")]
/// [InlineData("echo", """{"text":"world"}""")]
/// public async Task ToolsReturnValidSchema(string toolName, string paramsJson)
/// {
///     var parameters = JsonSerializer.Deserialize&lt;object&gt;(paramsJson);
///     await McpTest
///         .Server("http://localhost:8000/mcp")
///         .Tool(toolName)
///             .WithParams(parameters!)
///             .ExpectSchemaMatch()
///         .RunAsync();
/// }
/// </code>
/// </remarks>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public sealed class McpTheoryAttribute : TheoryAttribute
{
    public McpTheoryAttribute()
    {
    }

    /// <summary>
    /// The MCP server URL this test targets (informational).
    /// </summary>
    public string? Server { get; set; }
}
