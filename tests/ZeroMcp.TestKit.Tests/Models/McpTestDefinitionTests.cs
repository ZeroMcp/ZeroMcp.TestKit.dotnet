using System.Text.Json;
using ZeroMcp.TestKit.Models;

namespace ZeroMcp.TestKit.Tests.Models;

public class McpTestDefinitionTests
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        WriteIndented = true
    };

    [Fact]
    public void RoundTrip_MinimalDefinition()
    {
        var def = new McpTestDefinition
        {
            Server = "http://localhost:8000/mcp",
            Tests =
            [
                new McpTestCase
                {
                    Tool = "search",
                    Params = new { query = "hello" },
                }
            ],
        };

        var json = JsonSerializer.Serialize(def, Options);
        var parsed = JsonSerializer.Deserialize<McpTestDefinition>(json, Options);

        Assert.NotNull(parsed);
        Assert.Equal("1", parsed.Version);
        Assert.Equal("http://localhost:8000/mcp", parsed.Server);
        Assert.Single(parsed.Tests);
        Assert.Equal("search", parsed.Tests[0].Tool);
    }

    [Fact]
    public void RoundTrip_FullExpectation()
    {
        var def = new McpTestDefinition
        {
            Server = "http://localhost:8000/mcp",
            Tests =
            [
                new McpTestCase
                {
                    Tool = "search",
                    Params = new { query = "hello" },
                    Expect = new McpExpectation
                    {
                        SchemaValid = true,
                        Deterministic = true,
                        IgnorePaths = ["$.result.timestamp"],
                        TimeoutMs = 5000,
                    }
                }
            ],
            Config = new McpTestConfig
            {
                TimeoutMs = 10_000,
                DeterminismRuns = 5,
                ValidateProtocol = true,
                ValidateMetadata = true,
            }
        };

        var json = JsonSerializer.Serialize(def, Options);

        Assert.Contains("schema_valid", json);
        Assert.Contains("deterministic", json);
        Assert.Contains("ignore_paths", json);
        Assert.Contains("validate_protocol", json);

        var parsed = JsonSerializer.Deserialize<McpTestDefinition>(json, Options);
        Assert.NotNull(parsed);
        Assert.True(parsed.Tests[0].Expect.SchemaValid);
        Assert.True(parsed.Tests[0].Expect.Deterministic);
        Assert.Single(parsed.Tests[0].Expect.IgnorePaths!);
        Assert.NotNull(parsed.Config);
        Assert.Equal(5, parsed.Config.DeterminismRuns);
        Assert.True(parsed.Config.ValidateProtocol);
    }

    [Fact]
    public void RoundTrip_ErrorPath()
    {
        var def = new McpTestDefinition
        {
            Server = "http://localhost:8000/mcp",
            Tests =
            [
                new McpTestCase
                {
                    Tool = "nonexistent",
                    Expect = new McpExpectation
                    {
                        ExpectError = true,
                        ExpectErrorCode = -32601,
                    }
                }
            ],
        };

        var json = JsonSerializer.Serialize(def, Options);
        Assert.Contains("expect_error", json);
        Assert.Contains("-32601", json);

        var parsed = JsonSerializer.Deserialize<McpTestDefinition>(json, Options);
        Assert.NotNull(parsed);
        Assert.True(parsed.Tests[0].Expect.ExpectError);
        Assert.Equal(-32601, parsed.Tests[0].Expect.ExpectErrorCode);
    }

    [Fact]
    public void Serialization_OmitsDefaultValues()
    {
        var def = new McpTestDefinition
        {
            Server = "http://localhost:8000/mcp",
            Tests = [new McpTestCase { Tool = "echo" }],
        };

        var json = JsonSerializer.Serialize(def, Options);

        Assert.DoesNotContain("schema_valid", json);
        Assert.DoesNotContain("deterministic", json);
        Assert.DoesNotContain("ignore_paths", json);
        Assert.DoesNotContain("stream_min_chunks", json);
        Assert.DoesNotContain("expect_error_code", json);
    }

    [Fact]
    public void Deserialize_EngineJsonFormat()
    {
        var engineJson = """
        {
            "version": "1",
            "server": "http://localhost:8000/mcp",
            "tests": [
                {
                    "tool": "search",
                    "params": { "query": "hello" },
                    "expect": {
                        "schema_valid": true,
                        "deterministic": true,
                        "ignore_paths": ["$.result.timestamp", "$.result.id"]
                    }
                }
            ],
            "config": {
                "timeout_ms": 30000,
                "determinism_runs": 3
            }
        }
        """;

        var def = JsonSerializer.Deserialize<McpTestDefinition>(engineJson, Options);
        Assert.NotNull(def);
        Assert.Equal("search", def.Tests[0].Tool);
        Assert.True(def.Tests[0].Expect.SchemaValid);
        Assert.Equal(2, def.Tests[0].Expect.IgnorePaths!.Count);
        Assert.Equal(30_000, def.Config!.TimeoutMs);
    }
}
