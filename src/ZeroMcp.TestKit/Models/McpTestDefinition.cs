using System.Text.Json.Serialization;

namespace ZeroMcp.TestKit.Models;

/// <summary>
/// Top-level test definition document — the JSON contract consumed by the mcptest engine.
/// </summary>
public sealed class McpTestDefinition
{
    [JsonPropertyName("$schema")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Schema { get; set; } = "https://zeromcp.dev/schemas/testkit.v1.json";

    [JsonPropertyName("version")]
    public string Version { get; set; } = "1";

    [JsonPropertyName("server")]
    public string Server { get; set; } = "";

    [JsonPropertyName("tests")]
    public List<McpTestCase> Tests { get; set; } = [];

    [JsonPropertyName("config")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public McpTestConfig? Config { get; set; }
}

/// <summary>
/// A single test case targeting one tool call.
/// </summary>
public sealed class McpTestCase
{
    [JsonPropertyName("tool")]
    public string Tool { get; set; } = "";

    [JsonPropertyName("params")]
    public object? Params { get; set; }

    [JsonPropertyName("expect")]
    public McpExpectation Expect { get; set; } = new();

    [JsonPropertyName("_generated")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? Generated { get; set; }
}

/// <summary>
/// Expectations for a single test case.
/// </summary>
public sealed class McpExpectation
{
    [JsonPropertyName("schema_valid")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public bool SchemaValid { get; set; }

    [JsonPropertyName("deterministic")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public bool Deterministic { get; set; }

    [JsonPropertyName("ignore_paths")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string>? IgnorePaths { get; set; }

    [JsonPropertyName("stream_min_chunks")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? StreamMinChunks { get; set; }

    [JsonPropertyName("expect_error_code")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public long? ExpectErrorCode { get; set; }

    [JsonPropertyName("expect_error")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public bool ExpectError { get; set; }

    [JsonPropertyName("timeout_ms")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public long? TimeoutMs { get; set; }
}

/// <summary>
/// Global configuration for the test run.
/// </summary>
public sealed class McpTestConfig
{
    [JsonPropertyName("timeout_ms")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public long TimeoutMs { get; set; } = 30_000;

    [JsonPropertyName("determinism_runs")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public int DeterminismRuns { get; set; } = 3;

    [JsonPropertyName("retries")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public int Retries { get; set; }

    [JsonPropertyName("validate_protocol")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public bool ValidateProtocol { get; set; }

    [JsonPropertyName("validate_metadata")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public bool ValidateMetadata { get; set; }

    [JsonPropertyName("auto_error_tests")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public bool AutoErrorTests { get; set; }
}
