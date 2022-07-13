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

        public Token Peek(int forward)
        {
            if (!(index + forward < tokens.Count))
                Fail("Fail: Missing Tokens");

            return tokens[index + forward];
        }

        public void ExpectSymbol(char value)
        {
            if (PeekSymbol(value))
                Next();
            else
                Fail(Token.TokenType.SYMBOL, value.ToString());
        }

        public bool PeekSymbol(char value)
        {
            return Peek() is Symbol symbol && symbol.Value == value;
        }

        public bool PeekUnarySymbol()
        {
            return Peek() is Symbol symbol && Symbol.Unary.Contains(symbol.Value);
        }

        public void ExpectSymbol2(string value)
        {
            if (PeekSymbol2(value))
                Next();
            else
                Fail(Token.TokenType.SYMBOL2, value);
        }

        public bool PeekSymbol2(string value)
        {
            return Peek() is Symbol2 symbol2 && symbol2.Value == value;
        }

        public void ExpectKeyword(Keyword.KeywordTypes type)
        {
            if (PeekKeyword(type))
                Next();
            else
                Fail(Token.TokenType.KEYWORD, type.ToString());
        }

        public bool PeekKeyword(Keyword.KeywordTypes type)
        {
            return Peek() is Keyword keyword && keyword.KeywordType == type;
        }

        public void ExpectIdentifier(out string id)
        {
            id = string.Empty;
            if (Peek() is Identifier)
                id = ((Identifier)Next()).Value;
            else
                Fail(Token.TokenType.IDENTIFIER);
        }

        public void ExpectInteger(out int value)
        {
            value = 0;
            if (Peek() is Integer)
                value = ((Integer)Next()).Value;
            else
                Fail(Token.TokenType.INTEGER);
        }

        public void ExpectUnarySymbol(out char symbol)
        {
            symbol = char.MinValue;
            if (PeekUnarySymbol())
                symbol = ((Symbol)Next()).Value;
            else
                Fail(Token.TokenType.SYMBOL, "'-' or '~' or '!' or '+'");
        }

        public void Fail(string message)
        {
            failed = true;
            if (index == tokens.Count) index--;
            throw new UnexpectedValueException(message + " at Line: " + tokens[index].Line + ", Column: " + tokens[index].Column);
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