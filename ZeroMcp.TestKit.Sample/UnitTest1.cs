using System.ComponentModel;
using System.Threading.Tasks;
using Xunit;
using ZeroMcp.TestKit.Xunit;

namespace ZeroMcp.TestKit.Sample
{
    public class UnitTest1
    {
        [McpFact(Server = "http://localhost:8000/mcp")]
        public async Task ResponseTest()
        {
            var response = await McpTest
                    .Server("http://localhost:8000/mcp")
                    .Tool("search")
                        .WithParams(new { query = "hello" })
                        .ExpectSchemaMatch()
                        .RunAsync();
                        
            response.HasToolName("search")
                .HasValidSchema("search")
                .Passed();


        }
    }
}