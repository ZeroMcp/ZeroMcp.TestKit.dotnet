using ZeroMcp.TestKit.Models;

namespace ZeroMcp.TestKit;

/// <summary>
/// Builder for a single tool test case — configures params and expectations.
/// </summary>
public sealed class McpToolBuilder
{
    private readonly McpServerBuilder _parent;
    private readonly string _toolName;
    private object? _params;
    private readonly McpExpectation _expect = new();

    internal McpToolBuilder(McpServerBuilder parent, string toolName)
    {
        _parent = parent;
        _toolName = toolName;
    }

    /// <summary>
    /// Set the parameters to pass to the tool call.
    /// </summary>
    public McpToolBuilder WithParams(object parameters)
    {
        _params = parameters;
        return this;
    }

    /// <summary>
    /// Expect the tool output to conform to its declared JSON Schema.
    /// </summary>
    public McpToolBuilder ExpectSchemaMatch()
    {
        _expect.SchemaValid = true;
        return this;
    }

    /// <summary>
    /// Expect the tool to produce identical output across multiple runs.
    /// </summary>
    public McpToolBuilder ExpectDeterministic()
    {
        _expect.Deterministic = true;
        return this;
    }

    /// <summary>
    /// Add JSONPath expressions for fields to ignore during determinism comparison.
    /// </summary>
    public McpToolBuilder WithIgnorePaths(params string[] paths)
    {
        _expect.IgnorePaths ??= [];
        _expect.IgnorePaths.AddRange(paths);
        return this;
    }

    /// <summary>
    /// Expect at least the specified number of streaming chunks.
    /// </summary>
    public McpToolBuilder ExpectMinStreamChunks(int minChunks)
    {
        _expect.StreamMinChunks = minChunks;
        return this;
    }

    /// <summary>
    /// Expect the tool call to return an error response.
    /// </summary>
    public McpToolBuilder ExpectError()
    {
        _expect.ExpectError = true;
        return this;
    }

    /// <summary>
    /// Expect the tool call to return a specific JSON-RPC error code.
    /// </summary>
    public McpToolBuilder ExpectErrorCode(long code)
    {
        _expect.ExpectErrorCode = code;
        return this;
    }

    /// <summary>
    /// Override the timeout for this specific test case.
    /// </summary>
    public McpToolBuilder WithTimeout(TimeSpan timeout)
    {
        _expect.TimeoutMs = (long)timeout.TotalMilliseconds;
        return this;
    }

    /// <summary>
    /// Chain to add another tool test case.
    /// </summary>
    public McpToolBuilder Tool(string toolName)
    {
        return _parent.Tool(toolName);
    }

    /// <summary>
    /// Execute all configured tests. Throws <see cref="McpTestException"/> on failure.
    /// </summary>
    public Task<McpTestRunResult> RunAsync(CancellationToken cancellationToken = default)
    {
        return _parent.RunAsync(cancellationToken);
    }

    /// <summary>
    /// Execute all configured tests without throwing on failure.
    /// </summary>
    public Task<McpTestRunResult> RunWithoutThrowAsync(CancellationToken cancellationToken = default)
    {
        return _parent.RunWithoutThrowAsync(cancellationToken);
    }

    /// <summary>
    /// Build the test definition without executing.
    /// </summary>
    public McpTestDefinition BuildDefinition()
    {
        return _parent.BuildDefinition();
    }

    internal McpTestCase BuildTestCase()
    {
        return new McpTestCase
        {
            Tool = _toolName,
            Params = _params,
            Expect = _expect,
        };
    }
}
