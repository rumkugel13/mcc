namespace mcc
{
    class UnknownTokenException : Exception
    {
        public UnknownTokenException()
        {
        }

        public UnknownTokenException(string? message) : base(message)
        {
        }

        public UnknownTokenException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}