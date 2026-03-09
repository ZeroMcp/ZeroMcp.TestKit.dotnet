using System.ComponentModel;
using System.Threading.Tasks;
using Xunit;
using ZeroMcp.TestKit.Xunit;

namespace ZeroMcp.TestKit.Sample
{
    public class UnitTest1
    {
        [McpFact(Server = "http://localhost:41131/mcp")]
        public async Task ResponseTest()
        {
            var response = await McpTest
                    .Server("http://localhost:41131/mcp")
                    .Tool("get_customer")
                        .WithParams(new { id = 1 })
                        .ExpectSchemaMatch()
                        .RunAsync();
                        
            response.ForTool("get_customer")
                .HasToolName("get_customer")
                .HasValidSchema()
                .HasReturnProperty("id")
                .Passed();


        }
    }
}