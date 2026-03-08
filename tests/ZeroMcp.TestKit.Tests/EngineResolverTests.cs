namespace ZeroMcp.TestKit.Tests;

public class EngineResolverTests
{
    [Fact]
    public void Resolve_WithEnvVar_ReturnsPath()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            Environment.SetEnvironmentVariable("MCPTEST_PATH", tempFile);
            var resolved = EngineResolver.Resolve();
            Assert.Equal(Path.GetFullPath(tempFile), resolved);
        }
        finally
        {
            Environment.SetEnvironmentVariable("MCPTEST_PATH", null);
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void Resolve_WithMissingEnvVar_FallsThrough()
    {
        Environment.SetEnvironmentVariable("MCPTEST_PATH", null);

        // This will either find mcptest on PATH or throw with a descriptive message
        try
        {
            var resolved = EngineResolver.Resolve();
            Assert.True(File.Exists(resolved));
        }
        catch (McpTestException ex)
        {
            Assert.Contains("Could not find", ex.Message);
            Assert.Contains("MCPTEST_PATH", ex.Message);
        }
    }

    [Fact]
    public void Resolve_WithInvalidEnvVar_FallsThrough()
    {
        Environment.SetEnvironmentVariable("MCPTEST_PATH", "/nonexistent/path/mcptest");
        try
        {
            try
            {
                var resolved = EngineResolver.Resolve();
                // If it resolves, it found it on PATH or native assets
                Assert.True(File.Exists(resolved));
            }
            catch (McpTestException ex)
            {
                Assert.Contains("Could not find", ex.Message);
            }
        }
        finally
        {
            Environment.SetEnvironmentVariable("MCPTEST_PATH", null);
        }
    }
}
