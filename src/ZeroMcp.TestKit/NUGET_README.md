# ZeroMcp.TestKit

A fluent .NET API for testing [MCP (Model Context Protocol)](https://modelcontextprotocol.io/) servers. Define expectations in code and run them against any MCP endpoint — HTTP, WebSocket, or stdio.

Powered by the [mcptest](https://github.com/ZeroMcp/ZeroMcp.TeskKitEngine) engine (bundled as native assets).

## Install

```
dotnet add package ZeroMcp.TestKit
```

For xUnit integration (recommended):

```
dotnet add package ZeroMcp.TestKit.Xunit
```

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

`RunAsync()` throws `McpTestException` with detailed error messages if any check fails — integrates naturally with any test framework.

## What You Can Test

| Expectation | Method |
|---|---|
| Output matches declared JSON Schema | `.ExpectSchemaMatch()` |
| Identical output across N runs | `.ExpectDeterministic()` |
| Tool returns an error response | `.ExpectError()` / `.ExpectErrorCode(code)` |
| Streaming produces enough chunks | `.ExpectMinStreamChunks(n)` |
| Tool completes within a deadline | `.WithTimeout(TimeSpan)` |

## Fluent API

```csharp
await McpTest
    .Server("http://localhost:8000/mcp")
    .WithTimeout(TimeSpan.FromSeconds(30))
    .WithDeterminismRuns(5)
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

## Inspect Responses

Use `RunWithoutThrowAsync()` to get the result without throwing, then inspect tool responses:

```csharp
var result = await McpTest
    .Server("http://localhost:8000/mcp")
    .Tool("get_customer")
        .WithParams(new { id = 1 })
    .RunWithoutThrowAsync();

result.ForTool("get_customer")
    .Passed()
    .HasReturnProperty("id")
    .HasReturnProperty("name")
    .HasReturnValue("email", "alice@example.com");
```

## Engine Resolution

The `mcptest` binary is located automatically:

1. `MCPTEST_PATH` environment variable
2. NuGet native assets (`runtimes/{rid}/native/mcptest`) — bundled with this package
3. System `PATH`

## Links

- [GitHub Repository](https://github.com/ZeroMcp/ZeroMcp.TestKit.dotnet)
- [mcptest Engine](https://github.com/ZeroMcp/ZeroMcp.TeskKitEngine)
- [Model Context Protocol](https://modelcontextprotocol.io/)
