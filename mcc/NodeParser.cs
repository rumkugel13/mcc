namespace mcc
{
    class NodeParser
    {
        List<Token> tokens;
        int index;
        bool failed;

        public NodeParser(List<Token> tokens)
        {
            this.tokens = tokens;
            index = 0;
            failed = false;
        }

        public ASTProgramNode ParseProgram(string programName)
        {
            ASTFunctionNode function = ParseFunction();
            return new ASTProgramNode(programName, function);
        }

        public ASTFunctionNode ParseFunction()
        {
            ExpectKeyword(Keyword.KeywordTypes.INT);
            ExpectIdentifier(out string name);
            ExpectSymbol('(');
            ExpectSymbol(')');
            ExpectSymbol('{');
            ASTReturnNode returnNode = ParseReturn();
            ExpectSymbol('}');
            return new ASTFunctionNode(name, returnNode);
        }

        public ASTReturnNode ParseReturn()
        {
            ExpectKeyword(Keyword.KeywordTypes.RETURN);
            ASTExpressionNode exp = ParseExpression();
            ExpectSymbol(';');
            return new ASTReturnNode(exp);
        }

        public ASTConstantNode ParseConstant()
        {
            ExpectInteger(out int value);
            return new ASTConstantNode(value);
        }

        public ASTUnaryOpNode ParseUnaryOp()
        {
            ExpectUnarySymbol(out char symbol);
            ASTExpressionNode exp = ParseFactor();
            return new ASTUnaryOpNode(symbol, exp);
        }

        public ASTExpressionNode ParseFactor()
        {
            if (PeekUnarySymbol())
            {
                return ParseUnaryOp();
            }
            else if (Peek() is Integer)
            {
                return ParseConstant();
            }
            else if (PeekSymbol('('))
            {
                ExpectSymbol('(');
                ASTExpressionNode exp = ParseExpression();
                ExpectSymbol(')');
                return exp;
            }
            else
            {
                Fail("Expected UnaryOp or Integer");
                return new ASTNoExpressionNode();
            }
        }

        public ASTExpressionNode ParseTerm()
        {
            ASTExpressionNode exp = ParseFactor();
            while (PeekSymbol('*') || PeekSymbol('/') || PeekSymbol('%'))
            {
                ExpectBinarySymbol(out char binOp);
                ASTExpressionNode second = ParseFactor();
                exp = new ASTBinaryOpNode(binOp, exp, second);
            }
            return exp;
        }

        public ASTExpressionNode ParseExpression()
        {
            ASTExpressionNode exp = ParseTerm();
            while (PeekSymbol('+') || PeekSymbol('-'))
            {
                ExpectBinarySymbol(out char binOp);
                ASTExpressionNode second = ParseTerm();
                exp = new ASTBinaryOpNode(binOp, exp, second);
            }
            return exp;
        }

        private void ExpectBinarySymbol(out char value)
        {
            value = default;
            Token token = Next();
            if (token is Symbol symbol)
            {
                if (!Symbol.Binary.Contains(symbol.Value))
                {
                    Fail(Token.TokenType.SYMBOL, symbol.Value.ToString());
                }
                else
                {
                    value = symbol.Value;//.ToString();
                }
            }
            //else if (token is Symbol2 symbol2)
            //{
            //    if (!Symbol2.Dual.Contains(symbol2.Value))
            //    {
            //        Fail(Token.TokenType.SYMBOL2, symbol2.Value);
            //    }
            //    else
            //    {
            //        Value = symbol2.Value;
            //    }
            //}
            else
            {
                Fail(Token.TokenType.SYMBOL, "or Symbol2");
            }
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