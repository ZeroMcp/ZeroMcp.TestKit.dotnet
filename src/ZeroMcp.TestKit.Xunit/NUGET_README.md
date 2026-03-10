# ZeroMcp.TestKit.Xunit

xUnit integration for [ZeroMcp.TestKit](https://www.nuget.org/packages/ZeroMcp.TestKit) — test [MCP (Model Context Protocol)](https://modelcontextprotocol.io/) servers with first-class Visual Studio Test Explorer support.

## Install

```
dotnet add package ZeroMcp.TestKit.Xunit
```

This package depends on `ZeroMcp.TestKit` (installed automatically).

## Attributes

### [McpFact]

Marks a test as an MCP server test. Inherits from xUnit's `[Fact]`.

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

### [McpTheory]

Parameterized MCP tests. Inherits from xUnit's `[Theory]`.

```csharp
[McpTheory(DisplayName = "Lookup tools return valid schema")]
[InlineData("get_customer", 1)]
[InlineData("get_product", 1)]
[InlineData("get_order", 1)]
public async Task LookupById_SchemaValid(string toolName, int id)
{
    await McpTest
        .Server("http://localhost:8000/mcp")
        .Tool(toolName)
            .WithParams(new { id })
            .ExpectSchemaMatch()
        .RunAsync();
}
```

## McpAssert — Static Assertion Helpers

```csharp
var result = await McpTest
    .Server("http://localhost:8000/mcp")
    .Tool("search").WithParams(new { query = "hi" })
    .RunWithoutThrowAsync();

McpAssert.Passed(result);
McpAssert.ToolPassed(result, "search");
McpAssert.SchemaValid(result, "search");
McpAssert.Deterministic(result, "search");
McpAssert.ResponseHasProperty(result, "search", "name");
McpAssert.ResponseContains(result, "search", "name", "Alice");
```

## Fluent Assertion Chains

Scope to a specific tool with `ForTool()`, then chain assertions:

```csharp
var result = await McpTest
    .Server("http://localhost:8000/mcp")
    .Tool("get_customer").WithParams(new { id = 1 })
    .RunWithoutThrowAsync();

result.ForTool("get_customer")
    .Passed()
    .HasValidSchema()
    .HasReturnProperty("id")
    .HasReturnProperty("name")
    .HasReturnValue("email", "alice@example.com");
```

### Available Fluent Methods

| Method | Asserts |
|---|---|
| `.Passed()` | Tool test passed |
| `.Failed()` | Tool test failed |
| `.HasToolName(name)` | Tool name matches |
| `.HasValidSchema()` | Schema validation passed |
| `.IsDeterministic()` | Determinism check passed |
| `.HasReturnProperty(path)` | Response payload contains the property |
| `.HasReturnValue(path, value)` | Response payload property equals the value |

## Links

- [GitHub Repository](https://github.com/ZeroMcp/ZeroMcp.TestKit.dotnet)
- [ZeroMcp.TestKit on NuGet](https://www.nuget.org/packages/ZeroMcp.TestKit)
- [Model Context Protocol](https://modelcontextprotocol.io/)
