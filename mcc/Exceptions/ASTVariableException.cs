namespace mcc
{
    class ASTVariableException : Exception
    {
        public ASTVariableException()
        {
        }

        public ASTVariableException(string? message) : base(message)
        {
        }

        public ASTVariableException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}