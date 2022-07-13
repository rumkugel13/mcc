﻿namespace mcc
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

            List<ASTStatementNode> statements = new List<ASTStatementNode>();
            while (!PeekSymbol('}'))
            {
                statements.Add(ParseStatement());
            }
            ExpectSymbol('}');
            return new ASTFunctionNode(name, statements);
        }

        public ASTStatementNode ParseStatement()
        {
            if (PeekKeyword(Keyword.KeywordTypes.RETURN))
            {
                return ParseReturn();
            }
            else if (PeekKeyword(Keyword.KeywordTypes.INT))
            {
                return ParseDeclaration();
            }
            else
            {
                ASTAbstractExpressionNode exp = ParseExpression();
                ExpectSymbol(';');
                return new ASTExpressionNode(exp);
            }
        }

        public ASTDeclarationNode ParseDeclaration()
        {
            ExpectKeyword(Keyword.KeywordTypes.INT);
            ExpectIdentifier(out string id);

            if (PeekSymbol('='))
            {
                ExpectSymbol('=');
                ASTAbstractExpressionNode exp = ParseExpression();
                ExpectSymbol(';');
                return new ASTDeclarationNode(id, exp);
            }
            else
            {
                ExpectSymbol(';');
                return new ASTDeclarationNode(id);
            }
        }

        public ASTReturnNode ParseReturn()
        {
            ExpectKeyword(Keyword.KeywordTypes.RETURN);
            ASTAbstractExpressionNode exp = ParseExpression();
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
            ASTAbstractExpressionNode exp = ParseFactor();
            return new ASTUnaryOpNode(symbol, exp);
        }

        public ASTAbstractExpressionNode ParseFactor()
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
                ASTAbstractExpressionNode exp = ParseExpression();
                ExpectSymbol(')');
                return exp;
            }
            else if (Peek() is Identifier)
            {
                ExpectIdentifier(out string id);
                return new ASTVariableNode(id);
            }
            else
            {
                Fail("Expected UnaryOp or Integer or Variable");
                return new ASTNoExpressionNode();
            }
        }

        public ASTAbstractExpressionNode ParseTerm()
        {
            ASTAbstractExpressionNode exp = ParseFactor();
            while (PeekSymbol('*') || PeekSymbol('/') || PeekSymbol('%'))
            {
                ExpectBinarySymbol(out string binOp);
                ASTAbstractExpressionNode second = ParseFactor();
                exp = new ASTBinaryOpNode(binOp, exp, second);
            }
            return exp;
        }

        public ASTAbstractExpressionNode ParseAdditiveExpression()
        {
            ASTAbstractExpressionNode exp = ParseTerm();
            while (PeekSymbol('+') || PeekSymbol('-'))
            {
                ExpectBinarySymbol(out string binOp);
                ASTAbstractExpressionNode second = ParseTerm();
                exp = new ASTBinaryOpNode(binOp, exp, second);
            }
            return exp;
        }

        public ASTAbstractExpressionNode ParseShiftExpression()
        {
            ASTAbstractExpressionNode exp = ParseAdditiveExpression();
            while (PeekSymbol2("<<") || PeekSymbol2(">>"))
            {
                ExpectBinarySymbol(out string binOp);
                ASTAbstractExpressionNode second = ParseAdditiveExpression();
                exp = new ASTBinaryOpNode(binOp, exp, second);
            }
            return exp;
        }

        public ASTAbstractExpressionNode ParseRelationalExpression()
        {
            ASTAbstractExpressionNode exp = ParseShiftExpression();
            while (PeekSymbol('<') || PeekSymbol('>') ||
                   PeekSymbol2("<=") || PeekSymbol2(">="))
            {
                ExpectBinarySymbol(out string binOp);
                ASTAbstractExpressionNode second = ParseShiftExpression();
                exp = new ASTBinaryOpNode(binOp, exp, second);
            }
            return exp;
        }

        public ASTAbstractExpressionNode ParseEqualityExpression()
        {
            ASTAbstractExpressionNode exp = ParseRelationalExpression();
            while (PeekSymbol2("!=") || PeekSymbol2("=="))
            {
                ExpectBinarySymbol(out string binOp);
                ASTAbstractExpressionNode second = ParseRelationalExpression();
                exp = new ASTBinaryOpNode(binOp, exp, second);
            }
            return exp;
        }

        public ASTAbstractExpressionNode ParseBitwiseAndExpression()
        {
            ASTAbstractExpressionNode exp = ParseEqualityExpression();
            while (PeekSymbol('&'))
            {
                ExpectBinarySymbol(out string binOp);
                ASTAbstractExpressionNode second = ParseEqualityExpression();
                exp = new ASTBinaryOpNode(binOp, exp, second);
            }
            return exp;
        }

        public ASTAbstractExpressionNode ParseBitwiseXorExpression()
        {
            ASTAbstractExpressionNode exp = ParseBitwiseAndExpression();
            while (PeekSymbol('^'))
            {
                ExpectBinarySymbol(out string binOp);
                ASTAbstractExpressionNode second = ParseBitwiseAndExpression();
                exp = new ASTBinaryOpNode(binOp, exp, second);
            }
            return exp;
        }

        public ASTAbstractExpressionNode ParseBitwiseOrExpression()
        {
            ASTAbstractExpressionNode exp = ParseBitwiseXorExpression();
            while (PeekSymbol('|'))
            {
                ExpectBinarySymbol(out string binOp);
                ASTAbstractExpressionNode second = ParseBitwiseXorExpression();
                exp = new ASTBinaryOpNode(binOp, exp, second);
            }
            return exp;
        }

        public ASTAbstractExpressionNode ParseLogicalAndExpression()
        {
            ASTAbstractExpressionNode exp = ParseBitwiseOrExpression();
            while (PeekSymbol2("&&"))
            {
                ExpectBinarySymbol(out string binOp);
                ASTAbstractExpressionNode second = ParseBitwiseOrExpression();
                exp = new ASTBinaryOpNode(binOp, exp, second);
            }
            return exp;
        }

        public ASTAbstractExpressionNode ParseLogicalOrExpression()
        {
            ASTAbstractExpressionNode exp = ParseLogicalAndExpression();
            while (PeekSymbol2("||"))
            {
                ExpectBinarySymbol(out string binOp);
                ASTAbstractExpressionNode second = ParseLogicalAndExpression();
                exp = new ASTBinaryOpNode(binOp, exp, second);
            }
            return exp;
        }

        public ASTAbstractExpressionNode ParseExpression()
        {
            if (Peek() is Identifier && Peek(1) is Symbol symbol && symbol.Value == '=')
            {
                return ParseAssignment();
            }
            else
            {
                return ParseLogicalOrExpression();
            }
        }

        public ASTAbstractExpressionNode ParseAssignment()
        {
            ExpectIdentifier(out string id);
            ExpectSymbol('=');
            ASTAbstractExpressionNode expression = ParseExpression();
            return new ASTAssignNode(id, expression);
        }

        private void ExpectBinarySymbol(out string value)
        {
            value = string.Empty;
            Token token = Next();
            if (token is Symbol symbol)
            {
                if (!Symbol.Binary.Contains(symbol.Value))
                {
                    Fail(Token.TokenType.SYMBOL, symbol.Value.ToString());
                }
                else
                {
                    value = symbol.Value.ToString();
                }
            }
            else if (token is Symbol2 symbol2)
            {
                if (!Symbol2.Dual.Contains(symbol2.Value))
                {
                    Fail(Token.TokenType.SYMBOL2, symbol2.Value);
                }
                else
                {
                    value = symbol2.Value;
                }
            }
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