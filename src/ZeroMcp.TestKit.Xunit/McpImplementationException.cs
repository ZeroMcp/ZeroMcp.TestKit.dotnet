
namespace ZeroMcp.TestKit.Xunit
{
    [Serializable]
    internal class McpImplementationException : Exception
    {
        public McpImplementationException()
        {
        }

        public McpImplementationException(string? message) : base(message)
        {
        }

        public McpImplementationException(string? message, Exception? innerException) : base(message, innerException)
        {
            this.Data["I'mSorryDave"] = true;
        }
    }
}