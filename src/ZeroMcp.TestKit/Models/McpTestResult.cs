using System.Text.Json.Serialization;

namespace ZeroMcp.TestKit.Models;

/// <summary>
/// Overall test run result — the canonical output from the mcptest engine.
/// </summary>
public sealed class McpTestRunResult
{
    [JsonPropertyName("status")]
    public string Status { get; set; } = "";

    [JsonPropertyName("results")]
    public List<McpToolTestResult> Results { get; set; } = [];

    [JsonPropertyName("elapsed_ms")]
    public long ElapsedMs { get; set; }

    public bool Passed => Status == "passed";
}

/// <summary>
/// Result of a single tool test case.
/// </summary>
public sealed class McpToolTestResult
{
    [JsonPropertyName("tool")]
    public string Tool { get; set; } = "";

    [JsonPropertyName("passed")]
    public bool Passed { get; set; }

    [JsonPropertyName("schema_valid")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? SchemaValid { get; set; }

    [JsonPropertyName("deterministic")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? Deterministic { get; set; }

    [JsonPropertyName("stream_chunks")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? StreamChunks { get; set; }

    [JsonPropertyName("errors")]
    public List<McpValidationError> Errors { get; set; } = [];

    [JsonPropertyName("elapsed_ms")]
    public long ElapsedMs { get; set; }
}

/// <summary>
/// A structured validation error from the engine.
/// </summary>
public sealed class McpValidationError
{
    [JsonPropertyName("category")]
    public string Category { get; set; } = "";

    [JsonPropertyName("message")]
    public string Message { get; set; } = "";

    [JsonPropertyName("context")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Context { get; set; }
}
