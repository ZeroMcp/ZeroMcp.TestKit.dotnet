using ZeroMcp.TestKit.Models;

namespace ZeroMcp.TestKit;

/// <summary>
/// Builder for server-level configuration and tool test chains.
/// </summary>
public sealed class McpServerBuilder
{
    private readonly string _serverUrl;
    private readonly List<McpToolBuilder> _tools = [];
    private readonly McpTestConfig _config = new();
    private McpTestRunner? _runner;

    internal McpServerBuilder(string serverUrl)
    {
        _serverUrl = serverUrl;
    }

    /// <summary>
    /// Set a custom mcptest engine path instead of auto-resolving.
    /// </summary>
    public McpServerBuilder WithEnginePath(string enginePath)
    {
        _runner = new McpTestRunner(enginePath);
        return this;
    }

    /// <summary>
    /// Override the default timeout for all test cases.
    /// </summary>
    public McpServerBuilder WithTimeout(TimeSpan timeout)
    {
        _config.TimeoutMs = (long)timeout.TotalMilliseconds;
        return this;
    }

    /// <summary>
    /// Set the number of determinism re-runs.
    /// </summary>
    public McpServerBuilder WithDeterminismRuns(int runs)
    {
        _config.DeterminismRuns = runs;
        return this;
    }

    /// <summary>
    /// Enable MCP protocol validation (handshake + JSON-RPC frame checks).
    /// </summary>
    public McpServerBuilder ValidateProtocol()
    {
        _config.ValidateProtocol = true;
        return this;
    }

    /// <summary>
    /// Enable tool metadata validation (name, description, inputSchema).
    /// </summary>
    public McpServerBuilder ValidateMetadata()
    {
        _config.ValidateMetadata = true;
        return this;
    }

    /// <summary>
    /// Auto-generate error-path tests (unknown tool, malformed params).
    /// </summary>
    public McpServerBuilder WithAutoErrorTests()
    {
        _config.AutoErrorTests = true;
        return this;
    }

    /// <summary>
    /// Start defining a test case for the specified tool.
    /// </summary>
    public McpToolBuilder Tool(string toolName)
    {
        var builder = new McpToolBuilder(this, toolName);
        _tools.Add(builder);
        return builder;
    }

    /// <summary>
    /// Build the test definition (useful for inspection or custom execution).
    /// </summary>
    public McpTestDefinition BuildDefinition()
    {
        return new McpTestDefinition
        {
            Server = _serverUrl,
            Tests = _tools.Select(t => t.BuildTestCase()).ToList(),
            Config = _config,
        };
    }

    /// <summary>
    /// Execute all configured tests against the MCP server.
    /// Throws <see cref="McpTestException"/> if any test fails.
    /// </summary>
    public async Task<McpTestRunResult> RunAsync(CancellationToken cancellationToken = default)
    {
        var definition = BuildDefinition();
        var runner = _runner ?? new McpTestRunner();
        var result = await runner.RunAsync(definition, cancellationToken);

        if (!result.Passed)
            throw McpTestException.FromResult(result);

        return result;
    }

    /// <summary>
    /// Execute all configured tests and return the result without throwing on failure.
    /// </summary>
    public async Task<McpTestRunResult> RunWithoutThrowAsync(CancellationToken cancellationToken = default)
    {
        var definition = BuildDefinition();
        var runner = _runner ?? new McpTestRunner();
        return await runner.RunAsync(definition, cancellationToken);
    }
}
