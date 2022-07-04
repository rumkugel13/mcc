namespace mcc
{
    class Parser
    {
        List<Token> tokens;
        int index;
        bool failed;

        public Parser(List<Token> tokens)
        {
            this.tokens = tokens;
            index = 0;
            failed = false;
        }

        public bool HasMoreTokens()
        {
            return index < tokens.Count;
        }

        public Token Next()
        {
            if (!HasMoreTokens())
            {
                failed = true;
                throw new InvalidDataException("Fail: Missing Tokens" + " at " + index);
            }

            return tokens[index++];
        }

        public Token Peek()
        {
            if (!HasMoreTokens())
            {
                failed = true;
                throw new InvalidDataException("Fail: Missing Tokens" + " at " + index);
            }

            return tokens[index];
        }

        public void Fail(Token.TokenType expected)
        {
            failed = true;
            throw new InvalidDataException("Fail: Expected " + expected + " at " + index);
        }

        public void Fail(Token.TokenType expected, string value)
        {
            failed = true;
            throw new InvalidDataException("Fail: Expected " + expected + " with Value '" + value + "'" + " at " + index);
        }

        public bool Failed()
        {
            return failed;
        }
    }
}