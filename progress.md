# Progress

## 2026-03-08

### Implemented `HasReturnProperty` / `McpAssert.HasProperty`

- **Added `Response` field to `McpToolTestResult`** (`Models/McpTestResult.cs`)
  - `JsonElement?` property to capture the raw tool response from the mcptest engine output.
  - Allows post-run inspection of actual tool return data.

- **Implemented `McpAssert.HasProperty`** (`McpAssert.cs`)
  - `HasProperty(result, property)` — asserts that at least one tool response contains the named property.
  - `HasProperty(result, toolName, property)` — asserts a specific tool's response contains the named property.
  - Produces clear failure messages including the property name and tools checked.

- **Cleaned up `McpAssert.cs`**
  - Removed unused `System.Security.Cryptography.X509Certificates` import.
  - Added `System.Text.Json` import for `JsonElement` / `JsonValueKind`.

- **`McpFluentAssertions.HasReturnProperty`** now works — it delegates to the implemented `McpAssert.HasProperty`.

- **Note:** `McpImplementationException` class is no longer referenced by any code and could be removed.

### Added MCP response envelope unwrapping (`ExtractPayload`)

- **Added `McpAssert.ExtractPayload(JsonElement)`** — unwraps the MCP response envelope (`content[0].text`) and parses the inner JSON string into a `JsonElement`. Falls back to the raw response if the envelope structure isn't recognised.
- **Updated all property/value assertion methods** to call `ExtractPayload` before `NavigatePath`:
  - `McpAssert.ResponseContains`
  - `McpAssert.ResponseHasProperty`
  - `McpFluentAssertions.HasReturnProperty`
  - `McpFluentAssertions.HasReturnValue`
- **Why:** The engine returns the MCP protocol envelope (`{ content: [{ text: "...", type: "text" }], isError: false }`). Property assertions like `.HasReturnProperty("id")` should navigate the business payload (`{"id":1,"name":"Alice",...}`), not the protocol wrapper.
