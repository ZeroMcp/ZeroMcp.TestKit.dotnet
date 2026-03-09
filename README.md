# ZeroMcp.TestKit — .NET DSL

A fluent .NET API for testing MCP (Model Context Protocol) servers. Wraps the
[mcptest](https://github.com/ZeroMcp/ZeroMcp.TestKitEngine) Rust engine and
integrates with xUnit for Visual Studio Test Explorer support.

## Quick Start

```csharp
using ZeroMcp.TestKit;

await McpTest
    .Server("http://localhost:8000/mcp")
    .Tool("search")
        .WithParams(new { query = "hello" })
        .ExpectSchemaMatch()
        .ExpectDeterministic()
    .RunAsync();
```

## Packages

| Package | Purpose |
|---------|---------|
| `ZeroMcp.TestKit` | Core DSL, models, engine runner |
| `ZeroMcp.TestKit.Xunit` | xUnit attributes (`[McpFact]`, `[McpTheory]`) and `McpAssert` |

## Engine Resolution

The `mcptest` binary is located automatically in this order:

1. `MCPTEST_PATH` environment variable (full path to binary)
2. NuGet native assets (`runtimes/{rid}/native/mcptest`)
3. System `PATH`

Override per-builder: `.WithEnginePath("path/to/mcptest")`

## Fluent API

```csharp
await McpTest
    .Server("http://localhost:8000/mcp")
    .WithTimeout(TimeSpan.FromSeconds(30))
    .WithDeterminismRuns(5)
    .ValidateProtocol()
    .ValidateMetadata()
    .WithAutoErrorTests()
    .Tool("search")
        .WithParams(new { query = "hello" })
        .ExpectSchemaMatch()
        .ExpectDeterministic()
        .WithIgnorePaths("$.result.timestamp")
    .Tool("echo")
        .WithParams(new { text = "world" })
        .ExpectSchemaMatch()
    .RunAsync();
```

### Key Methods

**McpServerBuilder** (from `McpTest.Server(url)`):
- `.WithTimeout(TimeSpan)` — global timeout
- `.WithDeterminismRuns(int)` — number of re-runs for determinism checks
- `.ValidateProtocol()` — enable MCP handshake + JSON-RPC frame validation
- `.ValidateMetadata()` — validate tool name, description, inputSchema
- `.WithAutoErrorTests()` — auto-generate error-path tests
- `.Tool(name)` — add a tool test case

**McpToolBuilder** (from `.Tool(name)`):
- `.WithParams(object)` — tool call parameters
- `.ExpectSchemaMatch()` — validate output against declared schema
- `.ExpectDeterministic()` — assert identical output across runs
- `.WithIgnorePaths(params string[])` — JSONPath fields to skip in determinism
- `.ExpectError()` / `.ExpectErrorCode(long)` — error-path testing
- `.ExpectMinStreamChunks(int)` — streaming validation
- `.WithTimeout(TimeSpan)` — per-tool timeout override

**Execution:**
- `.RunAsync()` — execute and throw `McpTestException` on failure
- `.RunWithoutThrowAsync()` — execute and return result without throwing

## xUnit Integration

```csharp
using ZeroMcp.TestKit;
using ZeroMcp.TestKit.Xunit;

public class MyMcpServerTests
{
    [McpFact(DisplayName = "search returns valid schema")]
    public async Task SearchToolSchemaValid()
    {
        await McpTest
            .Server("http://localhost:8000/mcp")
            .Tool("search")
                .WithParams(new { query = "hello" })
                .ExpectSchemaMatch()
            .RunAsync();
    }
}
```

### McpAssert Helpers

```csharp
var result = await McpTest
    .Server("http://localhost:8000/mcp")
    .Tool("search").WithParams(new { query = "hi" }).ExpectSchemaMatch()
    .RunWithoutThrowAsync();

McpAssert.Passed(result);
McpAssert.ToolPassed(result, "search");
McpAssert.SchemaValid(result, "search");
McpAssert.Deterministic(result, "search");
```

### Response Value Assertions

The engine now includes the raw MCP server response in each tool result. Use these helpers
to assert on specific response properties and values:

```csharp
var result = await McpTest
    .Server("http://localhost:8000/mcp")
    .Tool("search").WithParams(new { query = "hi" })
    .RunWithoutThrowAsync();

// Assert a specific property path has an expected value
McpAssert.ResponseContains(result, "search", "content[0].text", "hello world");

// Assert a property exists at a given path
McpAssert.ResponseHasProperty(result, "search", "content[0].type");

// Get the raw JsonElement for custom assertions
var response = McpAssert.GetResponse(result, "search");
Assert.Equal("text", response.GetProperty("content")[0].GetProperty("type").GetString());
```

### Fluent Assertion Chains

```csharp
var result = await McpTest
    .Server("http://localhost:8000/mcp")
    .Tool("search").WithParams(new { query = "hi" })
    .RunWithoutThrowAsync();

result
    .Passed()
    .HasToolName("search")
    .HasValidSchema("search")
    .HasReturnProperty("content")
    .HasReturnValue("search", "content[0].type", "text");
```

## Project Structure

```
dotnet/
├── ZeroMcp.TestKit.slnx
├── src/
│   ├── ZeroMcp.TestKit/           # Core DSL library
│   │   ├── Models/
│   │   │   ├── McpTestDefinition.cs
│   │   │   └── McpTestResult.cs
│   │   ├── McpTest.cs
│   │   ├── McpServerBuilder.cs
│   │   ├── McpToolBuilder.cs
│   │   ├── McpTestRunner.cs
│   │   ├── EngineResolver.cs
│   │   └── McpTestException.cs
│   └── ZeroMcp.TestKit.Xunit/     # xUnit integration
│       ├── McpFluentAssertions.cs
│       ├── McpFactAttribute.cs
│       ├── McpTheoryAttribute.cs
│       └── McpAssert.cs
└── tests/
    └── ZeroMcp.TestKit.Tests/     # Unit tests (33 passing)
        ├── Models/
        │   ├── McpTestDefinitionTests.cs
        │   └── McpTestResultTests.cs
        ├── FluentApiTests.cs
        ├── McpTestExceptionTests.cs
        ├── McpAssertResponseTests.cs
        └── EngineResolverTests.cs
```

## Building

```bash
dotnet build
dotnet test
```

## Requirements

- .NET 8.0 SDK
- `mcptest` binary (see Engine Resolution above)

## License

MIT
