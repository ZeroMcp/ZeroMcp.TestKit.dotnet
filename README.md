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
│       ├── McpFactAttribute.cs
│       ├── McpTheoryAttribute.cs
│       └── McpAssert.cs
└── tests/
    └── ZeroMcp.TestKit.Tests/     # Unit tests (24 passing)
        ├── Models/
        │   ├── McpTestDefinitionTests.cs
        │   └── McpTestResultTests.cs
        ├── FluentApiTests.cs
        ├── McpTestExceptionTests.cs
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
