using System.Text.Json;
using ZeroMcp.TestKit.Models;
using ZeroMcp.TestKit.Xunit;

namespace ZeroMcp.TestKit.Tests;

public class McpAssertResponseTests
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
    };

    private static McpTestRunResult MakeResultWithResponse(string responseJson)
    {
        var json = $$"""
        {
            "status": "passed",
            "results": [
                {
                    "tool": "search",
                    "passed": true,
                    "errors": [],
                    "response": {{responseJson}},
                    "elapsed_ms": 10
                }
            ],
            "elapsed_ms": 20
        }
        """;

        return JsonSerializer.Deserialize<McpTestRunResult>(json, Options)!;
    }

    [Fact]
    public void ResponseContains_MatchesSimpleProperty()
    {
        var result = MakeResultWithResponse("""{"content": [{"type": "text", "text": "hello"}], "isError": false}""");
        McpAssert.ResponseContains(result, "search", "isError", "False");
    }

    [Fact]
    public void ResponseContains_MatchesNestedArrayProperty()
    {
        var result = MakeResultWithResponse("""{"content": [{"type": "text", "text": "hello world"}]}""");
        McpAssert.ResponseContains(result, "search", "content[0].text", "hello world");
    }

    [Fact]
    public void ResponseContains_MatchesContentType()
    {
        var result = MakeResultWithResponse("""{"content": [{"type": "text", "text": "hi"}]}""");
        McpAssert.ResponseContains(result, "search", "content[0].type", "text");
    }

    [Fact]
    public void ResponseHasProperty_Succeeds()
    {
        var result = MakeResultWithResponse("""{"content": [{"type": "text", "text": "hi"}]}""");
        McpAssert.ResponseHasProperty(result, "search", "content");
        McpAssert.ResponseHasProperty(result, "search", "content[0].type");
    }

    [Fact]
    public void GetResponse_ReturnsJsonElement()
    {
        var result = MakeResultWithResponse("""{"content": [{"type": "text", "text": "data"}]}""");
        var response = McpAssert.GetResponse(result, "search");
        Assert.Equal(JsonValueKind.Object, response.ValueKind);
        Assert.True(response.TryGetProperty("content", out _));
    }

    [Fact]
    public void ResponseContains_FailsOnMismatch()
    {
        var result = MakeResultWithResponse("""{"content": [{"type": "text", "text": "hello"}]}""");
        Assert.ThrowsAny<Exception>(() =>
            McpAssert.ResponseContains(result, "search", "content[0].text", "wrong value"));
    }

    [Fact]
    public void ResponseHasProperty_FailsOnMissing()
    {
        var result = MakeResultWithResponse("""{"content": []}""");
        Assert.ThrowsAny<Exception>(() =>
            McpAssert.ResponseHasProperty(result, "search", "nonexistent"));
    }
}
