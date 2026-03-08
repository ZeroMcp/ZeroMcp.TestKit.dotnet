using System.Text.Json;
using ZeroMcp.TestKit.Models;

namespace ZeroMcp.TestKit.Tests.Models;

public class McpTestResultTests
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
    };

    [Fact]
    public void Deserialize_PassedResult()
    {
        var json = """
        {
            "status": "passed",
            "results": [
                {
                    "tool": "search",
                    "passed": true,
                    "schema_valid": true,
                    "deterministic": true,
                    "errors": [],
                    "elapsed_ms": 42
                }
            ],
            "elapsed_ms": 100
        }
        """;

        var result = JsonSerializer.Deserialize<McpTestRunResult>(json, Options);

        Assert.NotNull(result);
        Assert.True(result.Passed);
        Assert.Equal("passed", result.Status);
        Assert.Single(result.Results);
        Assert.True(result.Results[0].Passed);
        Assert.True(result.Results[0].SchemaValid);
        Assert.True(result.Results[0].Deterministic);
        Assert.Empty(result.Results[0].Errors);
        Assert.Equal(42, result.Results[0].ElapsedMs);
        Assert.Equal(100, result.ElapsedMs);
    }

    [Fact]
    public void Deserialize_FailedResult()
    {
        var json = """
        {
            "status": "failed",
            "results": [
                {
                    "tool": "search",
                    "passed": false,
                    "errors": [
                        {
                            "category": "schema",
                            "message": "Tool 'search': expected string, got number",
                            "context": "/result"
                        }
                    ],
                    "elapsed_ms": 15
                }
            ],
            "elapsed_ms": 50
        }
        """;

        var result = JsonSerializer.Deserialize<McpTestRunResult>(json, Options);

        Assert.NotNull(result);
        Assert.False(result.Passed);
        Assert.Single(result.Results);
        Assert.False(result.Results[0].Passed);
        Assert.Single(result.Results[0].Errors);
        Assert.Equal("schema", result.Results[0].Errors[0].Category);
        Assert.Equal("/result", result.Results[0].Errors[0].Context);
    }

    [Fact]
    public void Deserialize_MultipleToolResults()
    {
        var json = """
        {
            "status": "failed",
            "results": [
                { "tool": "echo", "passed": true, "errors": [], "elapsed_ms": 10 },
                { "tool": "search", "passed": false, "errors": [{"category": "determinism", "message": "differs"}], "elapsed_ms": 20 }
            ],
            "elapsed_ms": 30
        }
        """;

        var result = JsonSerializer.Deserialize<McpTestRunResult>(json, Options);

        Assert.NotNull(result);
        Assert.Equal(2, result.Results.Count);
        Assert.True(result.Results[0].Passed);
        Assert.False(result.Results[1].Passed);
    }

    [Fact]
    public void Passed_Property_ReflectsStatus()
    {
        Assert.True(new McpTestRunResult { Status = "passed" }.Passed);
        Assert.False(new McpTestRunResult { Status = "failed" }.Passed);
        Assert.False(new McpTestRunResult { Status = "error" }.Passed);
    }
}
