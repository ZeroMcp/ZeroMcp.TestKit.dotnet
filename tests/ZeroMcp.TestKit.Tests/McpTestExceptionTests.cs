using ZeroMcp.TestKit.Models;

namespace ZeroMcp.TestKit.Tests;

public class McpTestExceptionTests
{
    [Fact]
    public void FromResult_ContainsFailureDetails()
    {
        var result = new McpTestRunResult
        {
            Status = "failed",
            ElapsedMs = 100,
            Results =
            [
                new McpToolTestResult
                {
                    Tool = "search",
                    Passed = true,
                    Errors = [],
                    ElapsedMs = 30,
                },
                new McpToolTestResult
                {
                    Tool = "broken",
                    Passed = false,
                    Errors =
                    [
                        new McpValidationError
                        {
                            Category = "schema",
                            Message = "expected string, got number",
                            Context = "/result",
                        }
                    ],
                    ElapsedMs = 40,
                }
            ]
        };

        var ex = McpTestException.FromResult(result);

        Assert.Contains("1 of 2", ex.Message);
        Assert.Contains("broken", ex.Message);
        Assert.Contains("schema", ex.Message);
        Assert.Contains("expected string", ex.Message);
        Assert.NotNull(ex.Result);
        Assert.Single(ex.Failures);
        Assert.Equal("broken", ex.Failures[0].Tool);
    }

    [Fact]
    public void FromResult_MultipleFailures()
    {
        var result = new McpTestRunResult
        {
            Status = "failed",
            ElapsedMs = 50,
            Results =
            [
                new McpToolTestResult
                {
                    Tool = "a",
                    Passed = false,
                    Errors = [new McpValidationError { Category = "protocol", Message = "bad frame" }],
                },
                new McpToolTestResult
                {
                    Tool = "b",
                    Passed = false,
                    Errors = [new McpValidationError { Category = "determinism", Message = "differs" }],
                }
            ]
        };

        var ex = McpTestException.FromResult(result);

        Assert.Equal(2, ex.Failures.Count);
        Assert.Contains("2 of 2", ex.Message);
    }

    [Fact]
    public void SimpleConstructor_WorksWithoutResult()
    {
        var ex = new McpTestException("Engine not found");
        Assert.Null(ex.Result);
        Assert.Empty(ex.Failures);
        Assert.Equal("Engine not found", ex.Message);
    }
}
