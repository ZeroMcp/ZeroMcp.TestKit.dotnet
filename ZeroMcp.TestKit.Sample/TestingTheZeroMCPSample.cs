using System.Text.Json;
using ZeroMcp.TestKit;
using ZeroMcp.TestKit.Models;
using ZeroMcp.TestKit.Xunit;

namespace ZeroMcp.TestKit.Sample;

/// <summary>
/// Example tests targeting the Orders API MCP server.
///
/// These demonstrate the full ZeroMcp.TestKit surface:
///   - RunAsync (throws on failure) vs RunWithoutThrowAsync (returns result)
///   - Fluent assertion chains via ForTool()
///   - Static McpAssert helpers
///   - [McpFact] and [McpTheory] attributes
///   - Schema validation, determinism, streaming, error-path testing
///   - Response property and value inspection
/// </summary>
public class TestingTheZeroMCPSample
{
    private const string ServerUrl = "http://localhost:41131/mcp";

    // ───────────────────────────────────────────────────────────────
    //  1. BASIC — RunAsync throws on failure, no manual assertions needed
    // ───────────────────────────────────────────────────────────────

    [McpFact(DisplayName = "get_customer returns valid schema")]
    public async Task GetCustomer_SchemaValid()
    {
        await McpTest
            .Server(ServerUrl)
            .Tool("get_customer")
                .WithParams(new { id = 1 })
                .ExpectSchemaMatch()
            .RunAsync();
    }

    [McpFact(DisplayName = "list_customers returns data")]
    public async Task ListCustomers_ReturnsData()
    {
        await McpTest
            .Server(ServerUrl)
            .Tool("list_customers")
            .RunAsync();
    }

    [McpFact(DisplayName = "get_product returns valid schema")]
    public async Task GetProduct_SchemaValid()
    {
        await McpTest
            .Server(ServerUrl)
            .Tool("get_product")
                .WithParams(new { id = 1 })
                .ExpectSchemaMatch()
            .RunAsync();
    }

    // ───────────────────────────────────────────────────────────────
    //  2. FLUENT ASSERTIONS — RunWithoutThrowAsync + ForTool() chain
    // ───────────────────────────────────────────────────────────────

    [McpFact(DisplayName = "get_customer response has expected properties")]
    public async Task GetCustomer_FluentPropertyCheck()
    {
        var result = await McpTest
            .Server(ServerUrl)
            .Tool("get_customer")
                .WithParams(new { id = 1 })
                .ExpectSchemaMatch()
            .RunWithoutThrowAsync();

        result.ForTool("get_customer")
            .Passed()
            .HasValidSchema()
            .HasReturnProperty("id")
            .HasReturnProperty("name")
            .HasReturnProperty("email");
    }

    [McpFact(DisplayName = "get_customer returns correct values for customer 1")]
    public async Task GetCustomer_FluentValueCheck()
    {
        var result = await McpTest
            .Server(ServerUrl)
            .Tool("get_customer")
                .WithParams(new { id = 1 })
                .ExpectSchemaMatch()
            .RunWithoutThrowAsync();

        result.ForTool("get_customer")
            .Passed()
            .HasReturnValue("id", "1")
            .HasReturnValue("name", "Alice")
            .HasReturnValue("email", "alice@example.com");
    }

    [McpFact(DisplayName = "get_order response contains order details")]
    public async Task GetOrder_FluentPropertyCheck()
    {
        var result = await McpTest
            .Server(ServerUrl)
            .Tool("get_order")
                .WithParams(new { id = 1 })
                .ExpectSchemaMatch()
            .RunWithoutThrowAsync();

        result.ForTool("get_order")
            .Passed()
            .HasReturnProperty("id")
            .HasReturnProperty("customerName")
            .HasReturnProperty("product")
            .HasReturnProperty("quantity");
    }

    // ───────────────────────────────────────────────────────────────
    //  3. STATIC McpAssert HELPERS
    // ───────────────────────────────────────────────────────────────

    [McpFact(DisplayName = "list_products passes (McpAssert)")]
    public async Task ListProducts_StaticAssert()
    {
        var result = await McpTest
            .Server(ServerUrl)
            .Tool("list_products")
            .RunWithoutThrowAsync();

        McpAssert.Passed(result);
        McpAssert.ToolPassed(result, "list_products");
    }

    // ───────────────────────────────────────────────────────────────
    //  4. DETERMINISM — same input should produce identical output
    // ───────────────────────────────────────────────────────────────

    [McpFact(DisplayName = "get_customer is deterministic across 3 runs")]
    public async Task GetCustomer_Deterministic()
    {
        await McpTest
            .Server(ServerUrl)
            .WithDeterminismRuns(3)
            .Tool("get_customer")
                .WithParams(new { id = 1 })
                .ExpectDeterministic()
            .RunAsync();
    }

    [McpFact(DisplayName = "list_products is deterministic")]
    public async Task ListProducts_Deterministic()
    {
        await McpTest
            .Server(ServerUrl)
            .WithDeterminismRuns(3)
            .Tool("list_products")
                .ExpectDeterministic()
            .RunAsync();
    }

    // ───────────────────────────────────────────────────────────────
    //  5. MULTIPLE TOOLS IN ONE RUN
    // ───────────────────────────────────────────────────────────────

    [McpFact(DisplayName = "customer + product + order tools pass in a single run")]
    public async Task MultiTool_SingleRun()
    {
        var result = await McpTest
            .Server(ServerUrl)
            .Tool("get_customer")
                .WithParams(new { id = 1 })
                .ExpectSchemaMatch()
            .Tool("get_product")
                .WithParams(new { id = 1 })
                .ExpectSchemaMatch()
            .Tool("get_order")
                .WithParams(new { id = 1 })
                .ExpectSchemaMatch()
            .RunWithoutThrowAsync();

        McpAssert.Passed(result);

        result.ForTool("get_customer").HasReturnProperty("name");
        result.ForTool("get_product").HasReturnProperty("name");
        result.ForTool("get_order").HasReturnProperty("product");
    }

    // ───────────────────────────────────────────────────────────────
    //  6. [McpTheory] — PARAMETERIZED TESTS
    // ───────────────────────────────────────────────────────────────

    [McpTheory(DisplayName = "List tools return data")]
    [InlineData("list_customers")]
    [InlineData("list_orders")]
    [InlineData("list_products")]
    public async Task ListTools_ReturnData(string toolName)
    {
        await McpTest
            .Server(ServerUrl)
            .Tool(toolName)
            .RunAsync();
    }

    [McpTheory(DisplayName = "Lookup by ID returns valid schema")]
    [InlineData("get_customer", 1)]
    [InlineData("get_product", 1)]
    [InlineData("get_order", 1)]
    public async Task LookupById_SchemaValid(string toolName, int id)
    {
        await McpTest
            .Server(ServerUrl)
            .Tool(toolName)
                .WithParams(new { id })
                .ExpectSchemaMatch()
            .RunAsync();
    }

    // ───────────────────────────────────────────────────────────────
    //  7. FILTERING — list_orders with optional status param
    // ───────────────────────────────────────────────────────────────

    [McpTheory(DisplayName = "list_orders filters by status")]
    [InlineData("pending")]
    [InlineData("shipped")]
    [InlineData("cancelled")]
    public async Task ListOrders_FilterByStatus(string status)
    {
        await McpTest
            .Server(ServerUrl)
            .Tool("list_orders")
                .WithParams(new { status })
            .RunAsync();
    }

    // ───────────────────────────────────────────────────────────────
    //  8. CUSTOMER ORDERS — nested route
    // ───────────────────────────────────────────────────────────────

    [McpFact(DisplayName = "get_customer_orders returns orders for customer 1")]
    public async Task GetCustomerOrders_ReturnsData()
    {
        await McpTest
            .Server(ServerUrl)
            .Tool("get_customer_orders")
                .WithParams(new { id = 1 })
            .RunAsync();
    }

    // ───────────────────────────────────────────────────────────────
    //  9. STREAMING — stream_orders
    // ───────────────────────────────────────────────────────────────

    [McpFact(DisplayName = "stream_orders produces at least 1 streaming chunk")]
    public async Task StreamOrders_MinChunks()
    {
        await McpTest
            .Server(ServerUrl)
            .Tool("stream_orders")
                .ExpectMinStreamChunks(1)
            .RunAsync();
    }

    // ───────────────────────────────────────────────────────────────
    //  10. TIMEOUT CONFIGURATION
    // ───────────────────────────────────────────────────────────────

    [McpFact(DisplayName = "get_customer completes within 10s")]
    public async Task GetCustomer_WithTimeout()
    {
        await McpTest
            .Server(ServerUrl)
            .WithTimeout(TimeSpan.FromSeconds(10))
            .Tool("get_customer")
                .WithParams(new { id = 1 })
                .ExpectSchemaMatch()
            .RunAsync();
    }

    [McpFact(DisplayName = "per-tool timeout override")]
    public async Task GetCustomer_PerToolTimeout()
    {
        await McpTest
            .Server(ServerUrl)
            .Tool("get_customer")
                .WithParams(new { id = 1 })
                .WithTimeout(TimeSpan.FromSeconds(5))
                .ExpectSchemaMatch()
            .RunAsync();
    }

    // ───────────────────────────────────────────────────────────────
    //  11. WRITE OPERATIONS
    // ───────────────────────────────────────────────────────────────

    [McpFact(DisplayName = "create_customer is callable")]
    public async Task CreateCustomer_Callable()
    {
        var result = await McpTest
            .Server(ServerUrl)
            .Tool("create_customer")
                .WithParams(new { name = "Test User", email = "test@example.com" })
            .RunWithoutThrowAsync();

        Assert.Single(result.Results);
        Assert.Equal("create_customer", result.Results[0].Tool);
    }

    [McpFact(DisplayName = "create_order is callable")]
    public async Task CreateOrder_Callable()
    {
        var result = await McpTest
            .Server(ServerUrl)
            .Tool("create_order")
                .WithParams(new { customerName = "Alice", product = "Widget", quantity = 3 })
            .RunWithoutThrowAsync();

        Assert.Single(result.Results);
        Assert.Equal("create_order", result.Results[0].Tool);
    }

    [McpFact(DisplayName = "create_product is callable")]
    public async Task CreateProduct_Callable()
    {
        var result = await McpTest
            .Server(ServerUrl)
            .Tool("create_product")
                .WithParams(new { name = "Gadget", price = 29.99 })
            .RunWithoutThrowAsync();

        Assert.Single(result.Results);
        Assert.Equal("create_product", result.Results[0].Tool);
    }

    // ───────────────────────────────────────────────────────────────
    //  12. UPDATE — change order status
    // ───────────────────────────────────────────────────────────────

    [McpFact(DisplayName = "update_order_status changes status to shipped")]
    public async Task UpdateOrderStatus_Shipped()
    {
        var result = await McpTest
            .Server(ServerUrl)
            .Tool("update_order_status")
                .WithParams(new { id = 1, status = "shipped" })
            .RunWithoutThrowAsync();

        result.ForTool("update_order_status")
            .Passed();
    }

    // ───────────────────────────────────────────────────────────────
    //  13. SECURE ENDPOINT — get_secure_order
    // ───────────────────────────────────────────────────────────────

    [McpFact(DisplayName = "get_secure_order fails without auth headers")]
    public async Task GetSecureOrder_NoAuth_Fails()
    {
        var result = await McpTest
            .Server(ServerUrl)
            .Tool("get_secure_order")
                .WithParams(new { id = 1 })
            .RunWithoutThrowAsync();

        result.ForTool("get_secure_order")
            .Failed();
    }

    // ───────────────────────────────────────────────────────────────
    //  14. GET ORDER WITH OPTIONAL PARAM
    // ───────────────────────────────────────────────────────────────

    [McpFact(DisplayName = "get_order with includeHistory flag")]
    public async Task GetOrder_WithHistory()
    {
        var result = await McpTest
            .Server(ServerUrl)
            .Tool("get_order")
                .WithParams(new { id = 1, includeHistory = true })
                .ExpectSchemaMatch()
            .RunWithoutThrowAsync();

        result.ForTool("get_order")
            .Passed()
            .HasReturnProperty("id");
    }

    // ───────────────────────────────────────────────────────────────
    //  15. FILE UPLOAD
    // ───────────────────────────────────────────────────────────────

    [McpFact(DisplayName = "upload_document accepts base64 content")]
    public async Task UploadDocument_Base64()
    {
        var content = Convert.ToBase64String("Hello, World!"u8.ToArray());

        var result = await McpTest
            .Server(ServerUrl)
            .Tool("upload_document")
                .WithParams(new
                {
                    title = "test.txt",
                    document = content,
                    document_filename = "test.txt",
                    document_content_type = "text/plain"
                })
            .RunWithoutThrowAsync();

        result.ForTool("upload_document")
            .Passed();
    }

    // ───────────────────────────────────────────────────────────────
    //  16. SCHEMA + DETERMINISM COMBINED
    // ───────────────────────────────────────────────────────────────

    [McpFact(DisplayName = "get_customer passes schema and determinism")]
    public async Task GetCustomer_SchemaAndDeterminism()
    {
        await McpTest
            .Server(ServerUrl)
            .WithDeterminismRuns(3)
            .Tool("get_customer")
                .WithParams(new { id = 1 })
                .ExpectSchemaMatch()
                .ExpectDeterministic()
            .RunAsync();
    }

    // ───────────────────────────────────────────────────────────────
    //  17. FAILED ASSERTION EXAMPLE — RunWithoutThrowAsync + Failed()
    // ───────────────────────────────────────────────────────────────

    [McpFact(DisplayName = "list_customers schema check fails as expected (array response)")]
    public async Task ListCustomers_SchemaFails()
    {
        var result = await McpTest
            .Server(ServerUrl)
            .Tool("list_customers")
                .ExpectSchemaMatch()
            .RunWithoutThrowAsync();

        result.ForTool("list_customers")
            .Failed();
    }
}
