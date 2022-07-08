namespace mcc
{
    class ASTFunctionException : Exception
    {
        public ASTFunctionException()
        {
        }

        public ASTFunctionException(string? message) : base(message)
        {
        }

        public ASTFunctionException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}