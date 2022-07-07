namespace mcc
{
    class ASTLoopScopeException : Exception
    {
        public ASTLoopScopeException()
        {
        }

        public ASTLoopScopeException(string? message) : base(message)
        {
        }

        public ASTLoopScopeException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}