namespace mcc
{
    class Parser
    {
        List<Token> tokens;
        int index;
        bool failed;
        string programName;
        int currLine, currColumn;

        public Parser(List<Token> tokens, string programName)
        {
            this.tokens = tokens;
            this.programName = programName;
            index = 0;
            failed = false;
        }

        public ASTProgramNode ParseProgram()
        {
            return ParseProgram(programName);
        }

        public ASTProgramNode ParseProgram(string programName)
        {
            List<ASTTopLevelItemNode> topLevelItems = new List<ASTTopLevelItemNode>();
            while(HasMoreTokens())
            {
                ASTTopLevelItemNode topLevelItem = ParseTopLevelItem();
                topLevelItems.Add(topLevelItem);
            }

            return new ASTProgramNode(programName, topLevelItems);
        }

        public ASTTopLevelItemNode ParseTopLevelItem()
        {
            if (Peek(2) is Symbol symbol && symbol.Value == '(')
            {
                return ParseFunction();
            }
            else
            {
                return ParseDeclaration();
            }
        }

        public ASTFunctionNode ParseFunction()
        {
            ExpectKeyword(Keyword.KeywordTypes.INT);
            ExpectIdentifier(out string name, out int line, out int column);
            ExpectSymbol('(');

            List<string> parameters = new List<string>();
            if (PeekKeyword(Keyword.KeywordTypes.INT))
            {
                ExpectKeyword(Keyword.KeywordTypes.INT);
                ExpectIdentifier(out string id);
                parameters.Add(id);

                while (PeekSymbol(','))
                {
                    ExpectSymbol(',');
                    ExpectKeyword(Keyword.KeywordTypes.INT);
                    ExpectIdentifier(out string id2);
                    parameters.Add(id2);
                }
            }

            ExpectSymbol(')');

            if (PeekSymbol('{'))
            {
                List<ASTBlockItemNode> blockItems = new List<ASTBlockItemNode>();
                ExpectSymbol('{');
                while (!PeekSymbol('}'))
                {
                    blockItems.Add(ParseBlockItem());
                }
                ExpectSymbol('}');
                return new ASTFunctionNode(name, parameters, blockItems) { LineNumber = line, LineCharacter = column};
            }
            else
            {
                ExpectSymbol(';');
                return new ASTFunctionNode(name, parameters) { LineNumber = line, LineCharacter = column};
            }
        }

        public ASTBlockItemNode ParseBlockItem()
        {
            if (PeekKeyword(Keyword.KeywordTypes.INT))
            {
                return ParseDeclaration();
            }
            else
            {
                return ParseStatement();
            }
        }

        public ASTCompundNode ParseCompound()
        {
            ExpectSymbol('{');

            List<ASTBlockItemNode> blockItems = new List<ASTBlockItemNode>();
            while (!PeekSymbol('}'))
            {
                blockItems.Add(ParseBlockItem());
            }
            ExpectSymbol('}');
            return new ASTCompundNode(blockItems);
        }

        public ASTWhileNode ParseWhile()
        {
            ExpectKeyword(Keyword.KeywordTypes.WHILE);
            ExpectSymbol('(');
            ASTAbstractExpressionNode exp = ParseExpression();
            ExpectSymbol(')');
            ASTStatementNode statement = ParseStatement();
            return new ASTWhileNode(exp, statement);
        }

        public ASTDoWhileNode ParseDoWhile()
        {
            ExpectKeyword(Keyword.KeywordTypes.DO);
            ASTStatementNode statement = ParseStatement();
            ExpectKeyword(Keyword.KeywordTypes.WHILE);
            ExpectSymbol('(');
            ASTAbstractExpressionNode exp = ParseExpression();
            ExpectSymbol(')');
            ExpectSymbol(';');
            return new ASTDoWhileNode(statement, exp);
        }

        public ASTBreakNode ParseBreak()
        {
            ExpectKeyword(Keyword.KeywordTypes.BREAK, out int line, out int column);
            ExpectSymbol(';');
            return new ASTBreakNode() { LineNumber = line, LineCharacter = column };
        }

        public ASTContinueNode ParseContinue()
        {
            ExpectKeyword(Keyword.KeywordTypes.CONTINUE, out int line, out int column);
            ExpectSymbol(';');
            return new ASTContinueNode() { LineNumber = line, LineCharacter = column };
        }

        public ASTForNode ParseFor()
        {
            ExpectKeyword(Keyword.KeywordTypes.FOR);
            ExpectSymbol('(');
            ASTAbstractExpressionNode init = ParseOptionalExpression();
            ExpectSymbol(';');
            ASTAbstractExpressionNode condition = ParseOptionalExpression();
            if (condition is ASTNoExpressionNode)
            {
                condition = new ASTConstantNode(1); // insert a true condition
            }

            ExpectSymbol(';');
            ASTAbstractExpressionNode post = ParseOptionalExpression();
            ExpectSymbol(')');
            ASTStatementNode statement = ParseStatement();
            return new ASTForNode(statement, init, condition, post);
        }

        public ASTForDeclarationNode ParseForDeclaration()
        {
            ExpectKeyword(Keyword.KeywordTypes.FOR);
            ExpectSymbol('(');
            ASTDeclarationNode decl = ParseDeclaration(); // includes ;
            ASTAbstractExpressionNode condition = ParseOptionalExpression();
            if (condition is ASTNoExpressionNode)
            {
                condition = new ASTConstantNode(1); // insert a true condition
            }

            ExpectSymbol(';');
            ASTAbstractExpressionNode post = ParseOptionalExpression();
            ExpectSymbol(')');
            ASTStatementNode statement = ParseStatement();
            return new ASTForDeclarationNode(statement, decl, condition, post);
        }

        public ASTAbstractExpressionNode ParseOptionalExpression()
        {
            if (!PeekSymbol(';') && !PeekSymbol(')'))
            {
                return ParseExpression();
            }
            else
            {
                return new ASTNoExpressionNode();
            }
        }

        public ASTStatementNode ParseStatement()
        {
            if (PeekKeyword(Keyword.KeywordTypes.RETURN))
            {
                return ParseReturn();
            }
            else if (PeekKeyword(Keyword.KeywordTypes.IF))
            {
                return ParseCondition();
            }
            else if (PeekKeyword(Keyword.KeywordTypes.DO))
            {
                return ParseDoWhile();
            }
            else if (PeekKeyword(Keyword.KeywordTypes.FOR))
            {
                if (Peek(2) is Keyword kw && kw.KeywordType == Keyword.KeywordTypes.INT)
                {
                    return ParseForDeclaration();
                }
                else
                {
                    return ParseFor();
                }
            }
            else if (PeekKeyword(Keyword.KeywordTypes.WHILE))
            {
                return ParseWhile();
            }
            else if (PeekKeyword(Keyword.KeywordTypes.BREAK))
            {
                return ParseBreak();
            }
            else if (PeekKeyword(Keyword.KeywordTypes.CONTINUE))
            {
                return ParseContinue();
            }
            else if (PeekSymbol('{'))
            {
                return ParseCompound();
            }
            else
            {
                ASTAbstractExpressionNode exp = ParseOptionalExpression();
                ExpectSymbol(';');
                return new ASTExpressionNode(exp);
            }
        }

        public ASTConditionNode ParseCondition()
        {
            ExpectKeyword(Keyword.KeywordTypes.IF);
            ExpectSymbol('(');
            ASTAbstractExpressionNode condition = ParseExpression();
            ExpectSymbol(')');
            ASTStatementNode ifBranch = ParseStatement();
            if (PeekKeyword(Keyword.KeywordTypes.ELSE))
            {
                ExpectKeyword(Keyword.KeywordTypes.ELSE);
                ASTStatementNode elseBranch = ParseStatement();
                return new ASTConditionNode(condition, ifBranch, elseBranch);
            }
            else
            {
                return new ASTConditionNode(condition, ifBranch);
            }
        }

        public ASTDeclarationNode ParseDeclaration()
        {
            ExpectKeyword(Keyword.KeywordTypes.INT);
            ExpectIdentifier(out string id, out int line, out int column);

            if (PeekSymbol('='))
            {
                ExpectSymbol('=');
                ASTAbstractExpressionNode exp = ParseExpression();
                ExpectSymbol(';');
                return new ASTDeclarationNode(id, exp) { LineNumber = line, LineCharacter = column };
            }
            else
            {
                ExpectSymbol(';');
                return new ASTDeclarationNode(id) { LineNumber = line, LineCharacter = column };
            }
        }

        public ASTReturnNode ParseReturn()
        {
            ExpectKeyword(Keyword.KeywordTypes.RETURN, out int line, out int column);
            ASTAbstractExpressionNode exp = ParseExpression();
            ExpectSymbol(';');
            return new ASTReturnNode(exp) { LineNumber = line, LineCharacter = column };
        }

        public ASTConstantNode ParseConstant()
        {
            ExpectInteger(out int value);
            return new ASTConstantNode(value) { LineNumber = currLine, LineCharacter = currColumn };
        }

        public ASTUnaryOpNode ParseUnaryOp()
        {
            ExpectUnarySymbol(out char symbol);
            ASTAbstractExpressionNode exp = ParseFactor();
            return new ASTUnaryOpNode(symbol, exp);
        }

        public ASTFunctionCallNode ParseFunctionCall()
        {
            ExpectIdentifier(out string id, out int line, out int column);
            ExpectSymbol('(');

            List<ASTAbstractExpressionNode> arguments = new List<ASTAbstractExpressionNode>();
            if (!PeekSymbol(')'))
            {
                arguments.Add(ParseExpression());
                while (PeekSymbol(','))
                {
                    ExpectSymbol(',');
                    arguments.Add(ParseExpression());
                }
            }

            ExpectSymbol(')');
            return new ASTFunctionCallNode(id, arguments) { LineNumber = line, LineCharacter = column };
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
                if (Peek(1) is Symbol symbol && symbol.Value == '(')
                {
                    return ParseFunctionCall();
                }
                else
                {
                    ExpectIdentifier(out string id, out int line, out int column);
                    return new ASTVariableNode(id) { LineNumber = line, LineCharacter = column };
                }
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

        public ASTAbstractExpressionNode ParseConditionalExpression()
        {
            ASTAbstractExpressionNode exp = ParseLogicalOrExpression();
            if (PeekSymbol('?'))
            {
                ExpectSymbol('?');
                ASTAbstractExpressionNode ifBranch = ParseExpression();
                ExpectSymbol(':');
                ASTAbstractExpressionNode elseBranch = ParseConditionalExpression();
                return new ASTConditionalExpressionNode(exp, ifBranch, elseBranch);
            }
            else
            {
                return exp;
            }
        }

        public ASTAbstractExpressionNode ParseExpression()
        {
            if (Peek() is Identifier && Peek(1) is Symbol symbol && symbol.Value == '=')
            {
                return ParseAssignment();
            }
            else
            {
                return ParseConditionalExpression();
            }
        }

        public ASTAssignNode ParseAssignment()
        {
            ExpectIdentifier(out string id, out int line, out int column);
            ExpectSymbol('=');
            ASTAbstractExpressionNode expression = ParseExpression();
            return new ASTAssignNode(id, expression) { LineNumber = line, LineCharacter = column };
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

            currLine = tokens[index].Line;
            currColumn = tokens[index].Column;

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
            {
                Fail("Fail: Missing Tokens");
            }

            return tokens[index + forward];
        }

        public void ExpectSymbol(char value)
        {
            if (PeekSymbol(value))
            {
                Next();
            }
            else
            {
                Fail(Token.TokenType.SYMBOL, value.ToString());
            }
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
            {
                Next();
            }
            else
            {
                Fail(Token.TokenType.SYMBOL2, value);
            }
        }

        public bool PeekSymbol2(string value)
        {
            return Peek() is Symbol2 symbol2 && symbol2.Value == value;
        }

        public void ExpectKeyword(Keyword.KeywordTypes type)
        {
            if (PeekKeyword(type))
            {
                Next();
            }
            else
            {
                Fail(Token.TokenType.KEYWORD, type.ToString());
            }
        }

        public void ExpectKeyword(Keyword.KeywordTypes type, out int line, out int column)
        {
            line = column = 0;
            if (PeekKeyword(type))
            {
                Next();
                line = currLine;
                column = currColumn;
            }
            else
            {
                Fail(Token.TokenType.KEYWORD, type.ToString());
            }
        }

        public bool PeekKeyword(Keyword.KeywordTypes type)
        {
            return Peek() is Keyword keyword && keyword.KeywordType == type;
        }

        public void ExpectIdentifier(out string id)
        {
            id = string.Empty;
            if (Peek() is Identifier)
            {
                id = ((Identifier)Next()).Value;
            }
            else
            {
                Fail(Token.TokenType.IDENTIFIER);
            }
        }

        public void ExpectIdentifier(out string id, out int line, out int column)
        {
            id = string.Empty;
            line = column = 0;
            if (Peek() is Identifier)
            {
                id = ((Identifier)Next()).Value;
                line = currLine;
                column = currColumn;
            }
            else
            {
                Fail(Token.TokenType.IDENTIFIER);
            }
        }

        public void ExpectInteger(out int value)
        {
            value = 0;
            if (Peek() is Integer)
            {
                value = ((Integer)Next()).Value;
            }
            else
            {
                Fail(Token.TokenType.INTEGER);
            }
        }

        public void ExpectUnarySymbol(out char symbol)
        {
            symbol = char.MinValue;
            if (PeekUnarySymbol())
            {
                symbol = ((Symbol)Next()).Value;
            }
            else
            {
                Fail(Token.TokenType.SYMBOL, "'-' or '~' or '!' or '+'");
            }
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