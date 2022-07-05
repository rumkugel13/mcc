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
                Fail("Fail: Missing Tokens");
            }

            return tokens[index++];
        }

        public Token Peek()
        {
            if (!HasMoreTokens())
            {
                Fail("Fail: Missing Tokens");
            }

            return tokens[index];
        }

        public Token PeekNext()
        {
            if (!(index + 1 < tokens.Count))
            {
                Fail("Fail: Missing Tokens");
            }

            return tokens[index + 1];
        }

        public void ExpectSymbol(char value)
        {
            Token next = Next();
            if (next is not Symbol symbol || symbol.Value != value)
                Fail(Token.TokenType.SYMBOL, value.ToString());
        }

        public bool PeekSymbol(char value)
        {
            return Peek() is Symbol symbol && symbol.Value == value;
        }

        public void ExpectSymbol2(string value)
        {
            Token next = Next();
            if (next is not Symbol2 symbol2 || symbol2.Value != value)
                Fail(Token.TokenType.SYMBOL, value);
        }

        public bool PeekSymbol2(string value)
        {
            return Peek() is Symbol2 symbol2 && symbol2.Value == value;
        }

        public void ExpectKeyword(Keyword.KeywordTypes type)
        {
            Token next = Next();
            if (next is not Keyword keyword || keyword.KeywordType != type)
                Fail(Token.TokenType.KEYWORD, type.ToString());
        }

        public bool PeekKeyword(Keyword.KeywordTypes type)
        {
            return Peek() is Keyword keyword && keyword.KeywordType == type;
        }

        public void Fail(string message)
        {
            failed = true;
            throw new InvalidDataException(message + " at " + index);
        }

        public void Fail(Token.TokenType expected)
        {
            Fail("Fail: Expected " + expected);
        }

        public void Fail(Token.TokenType expected, string value)
        {
            Fail("Fail: Expected " + expected + " with Value '" + value + "'");
        }

        public bool Failed()
        {
            return failed;
        }
    }
}