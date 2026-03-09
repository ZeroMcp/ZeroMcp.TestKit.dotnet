using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using ZeroMcp.TestKit.Models;

namespace ZeroMcp.TestKit.Xunit
{
    public static class McpFluentAssertions
    {
        public static McpTestRunResult HasToolName(this McpTestRunResult test, string toolName)
        {
            McpAssert.ToolPassed(test, ToolName);
            return test;
        }
        public static McpTestRunResult HasValidSchema(this McpTestRunResult test, string toolName)
        {
            McpAssert.SchemaValid(test, toolName);
            return test;
        }
        public static McpTestRunResult IsDeterministic(this McpTestRunResult test, string toolName)
        {
            McpAssert.Deterministic(test, toolName);
            return test;
        }
        public static McpTestRunResult HasReturnProperty(this McpTestRunResult test, string property)
        {
            McpAssert.HasProperty(test, property);
            return test;
        }
        public static McpTestRunResult Passed(this McpTestRunResult test)
        {
            McpAssert.Passed(test);
            return test;
        }

        public static McpTestRunResult Failed(this McpTestRunResult test)
        {
            if (test.Passed)
                Assert.Fail("Expected test to fail, but it passed.");
            return test;
        }
    }
}
