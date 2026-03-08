namespace ZeroMcp.TestKit;

/// <summary>
/// Entry point for the MCP test fluent API.
/// <code>
/// await McpTest
///     .Server("http://localhost:8000/mcp")
///     .Tool("search")
///         .WithParams(new { query = "hello" })
///         .ExpectSchemaMatch()
///         .ExpectDeterministic()
///     .RunAsync();
/// </code>
/// </summary>
public static class McpTest
{
    /// <summary>
    /// Start building a test against the specified MCP server endpoint.
    /// </summary>
    /// <param name="serverUrl">MCP server URL (http://, ws://, or stdio: command).</param>
    public static McpServerBuilder Server(string serverUrl)
    {
        return new McpServerBuilder(serverUrl);
    }
}
