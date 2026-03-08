using System.Text.Json;
using ZeroMcp.TestKit.Models;

namespace ZeroMcp.TestKit.Tests;

public class FluentApiTests
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        WriteIndented = true
    };

    [Fact]
    public void SingleTool_BuildsCorrectDefinition()
    {
        var def = McpTest
            .Server("http://localhost:8000/mcp")
            .Tool("search")
                .WithParams(new { query = "hello" })
                .ExpectSchemaMatch()
            .BuildDefinition();

        Assert.Equal("http://localhost:8000/mcp", def.Server);
        Assert.Single(def.Tests);
        Assert.Equal("search", def.Tests[0].Tool);
        Assert.True(def.Tests[0].Expect.SchemaValid);
    }

    [Fact]
    public void MultipleTools_BuildsAllTestCases()
    {
        var def = McpTest
            .Server("http://localhost:8000/mcp")
            .Tool("search")
                .WithParams(new { query = "hello" })
                .ExpectSchemaMatch()
                .ExpectDeterministic()
            .Tool("echo")
                .WithParams(new { text = "world" })
                .ExpectSchemaMatch()
            .BuildDefinition();

        Assert.Equal(2, def.Tests.Count);
        Assert.Equal("search", def.Tests[0].Tool);
        Assert.True(def.Tests[0].Expect.Deterministic);
        Assert.Equal("echo", def.Tests[1].Tool);
        Assert.True(def.Tests[1].Expect.SchemaValid);
    }

    [Fact]
    public void ErrorPath_SetsExpectations()
    {
        var def = McpTest
            .Server("http://localhost:8000/mcp")
            .Tool("nonexistent")
                .ExpectError()
                .ExpectErrorCode(-32601)
            .BuildDefinition();

        Assert.True(def.Tests[0].Expect.ExpectError);
        Assert.Equal(-32601, def.Tests[0].Expect.ExpectErrorCode);
    }

    [Fact]
    public void IgnorePaths_AreCollected()
    {
        var def = McpTest
            .Server("http://localhost:8000/mcp")
            .Tool("search")
                .WithParams(new { query = "hello" })
                .ExpectDeterministic()
                .WithIgnorePaths("$.result.timestamp", "$.result.id")
            .BuildDefinition();

        Assert.Equal(2, def.Tests[0].Expect.IgnorePaths!.Count);
        Assert.Contains("$.result.timestamp", def.Tests[0].Expect.IgnorePaths!);
        Assert.Contains("$.result.id", def.Tests[0].Expect.IgnorePaths!);
    }

    [Fact]
    public void GlobalConfig_IsApplied()
    {
        var def = McpTest
            .Server("http://localhost:8000/mcp")
            .WithTimeout(TimeSpan.FromSeconds(10))
            .WithDeterminismRuns(5)
            .ValidateProtocol()
            .ValidateMetadata()
            .WithAutoErrorTests()
            .Tool("search")
                .WithParams(new { query = "hello" })
            .BuildDefinition();

        Assert.NotNull(def.Config);
        Assert.Equal(10_000, def.Config.TimeoutMs);
        Assert.Equal(5, def.Config.DeterminismRuns);
        Assert.True(def.Config.ValidateProtocol);
        Assert.True(def.Config.ValidateMetadata);
        Assert.True(def.Config.AutoErrorTests);
    }

    [Fact]
    public void PerToolTimeout_Overrides()
    {
        var def = McpTest
            .Server("http://localhost:8000/mcp")
            .Tool("slow")
                .WithTimeout(TimeSpan.FromSeconds(60))
            .BuildDefinition();

        Assert.Equal(60_000, def.Tests[0].Expect.TimeoutMs);
    }

    [Fact]
    public void StreamMinChunks_IsSet()
    {
        var def = McpTest
            .Server("http://localhost:8000/mcp")
            .Tool("streaming")
                .ExpectMinStreamChunks(5)
            .BuildDefinition();

        Assert.Equal(5, def.Tests[0].Expect.StreamMinChunks);
    }

    [Fact]
    public void BuildDefinition_ProducesValidJson()
    {
        var def = McpTest
            .Server("http://localhost:8000/mcp")
            .ValidateProtocol()
            .Tool("search")
                .WithParams(new { query = "hello" })
                .ExpectSchemaMatch()
                .ExpectDeterministic()
                .WithIgnorePaths("$.result.timestamp")
            .Tool("echo")
                .WithParams(new { text = "hi" })
            .BuildDefinition();

        var json = JsonSerializer.Serialize(def, Options);

        // Verify it round-trips through the engine's expected format
        var parsed = JsonSerializer.Deserialize<McpTestDefinition>(json, Options);
        Assert.NotNull(parsed);
        Assert.Equal("1", parsed.Version);
        Assert.Equal(2, parsed.Tests.Count);
        Assert.True(parsed.Config!.ValidateProtocol);
    }

    [Fact]
    public void SpecExample_MatchesDocumentation()
    {
        // Match the exact example from InitialSpecDoc.md Section 2.2
        var def = McpTest
            .Server("http://localhost:8000/mcp")
            .Tool("search")
                .WithParams(new { query = "hello" })
                .ExpectSchemaMatch()
                .ExpectDeterministic()
            .BuildDefinition();

        Assert.Equal("http://localhost:8000/mcp", def.Server);
        Assert.Single(def.Tests);
        Assert.Equal("search", def.Tests[0].Tool);
        Assert.True(def.Tests[0].Expect.SchemaValid);
        Assert.True(def.Tests[0].Expect.Deterministic);
    }
}
