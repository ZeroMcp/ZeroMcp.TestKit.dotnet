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
        var result = MakeResultWithResponse("""{"content": [{"type": "text", "text": "{\"id\":1,\"name\":\"Alice\"}"}], "isError": false}""");
        McpAssert.ResponseContains(result, "search", "name", "Alice");
    }

    [Fact]
    public void ResponseContains_MatchesNestedArrayProperty()
    {
        var result = MakeResultWithResponse("""{"content": [{"type": "text", "text": "{\"items\":[{\"value\":\"hello world\"}]}"}]}""");
        McpAssert.ResponseContains(result, "search", "items[0].value", "hello world");
    }

    [Fact]
    public void ResponseContains_MatchesContentType()
    {
        var result = MakeResultWithResponse("""{"content": [{"type": "text", "text": "{\"type\":\"widget\",\"count\":5}"}]}""");
        McpAssert.ResponseContains(result, "search", "type", "widget");
    }

    [Fact]
    public void ResponseHasProperty_Succeeds()
    {
        var result = MakeResultWithResponse("""{"content": [{"type": "text", "text": "{\"id\":1,\"name\":\"Alice\"}"}]}""");
        McpAssert.ResponseHasProperty(result, "search", "id");
        McpAssert.ResponseHasProperty(result, "search", "name");
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
        var result = MakeResultWithResponse("""{"content": [{"type": "text", "text": "{\"name\":\"Alice\"}"}]}""");
        Assert.ThrowsAny<Exception>(() =>
            McpAssert.ResponseContains(result, "search", "name", "wrong value"));
    }

    [Fact]
    public void ResponseHasProperty_FailsOnMissing()
    {
        var result = MakeResultWithResponse("""{"content": [{"type": "text", "text": "{\"id\":1}"}]}""");
        Assert.ThrowsAny<Exception>(() =>
            McpAssert.ResponseHasProperty(result, "search", "nonexistent"));
    }
}
