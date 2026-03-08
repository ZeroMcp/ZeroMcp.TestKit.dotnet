using Xunit;

namespace ZeroMcp.TestKit.Xunit;

/// <summary>
/// Marks a test method as an MCP server test. Inherits from xUnit's [Fact].
/// Use this to identify MCP-specific tests in Visual Studio Test Explorer.
/// </summary>
/// <remarks>
/// <code>
/// [McpFact(DisplayName = "search tool returns valid schema")]
/// public async Task SearchToolSchemaValid()
/// {
///     await McpTest
///         .Server("http://localhost:8000/mcp")
///         .Tool("search")
///             .WithParams(new { query = "hello" })
///             .ExpectSchemaMatch()
///         .RunAsync();
/// }
/// </code>
/// </remarks>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public sealed class McpFactAttribute : FactAttribute
{
    public McpFactAttribute()
    {
    }

    /// <summary>
    /// The MCP server URL this test targets (informational, for Test Explorer grouping).
    /// </summary>
    public string? Server { get; set; }
}
