namespace mcc
{
    class Parser
    {
        readonly IReadOnlyList<Token> tokens;
        int index;
        bool failed;
        string programName;
        int currLine, currColumn;

        public Parser(IReadOnlyList<Token> tokens, string programName)
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

        private ASTProgramNode ParseProgram(string programName)
        {
            List<ASTTopLevelItemNode> topLevelItems = new List<ASTTopLevelItemNode>();
            while(HasMoreTokens())
            {
                ASTTopLevelItemNode topLevelItem = ParseTopLevelItem();
                topLevelItems.Add(topLevelItem);
            }

            return new ASTProgramNode(programName, topLevelItems);
        }

        private ASTTopLevelItemNode ParseTopLevelItem()
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

        private ASTFunctionNode ParseFunction()
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

        private ASTBlockItemNode ParseBlockItem()
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

        private ASTCompundNode ParseCompound()
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

        private ASTWhileNode ParseWhile()
        {
            ExpectKeyword(Keyword.KeywordTypes.WHILE);
            ExpectSymbol('(');
            ASTAbstractExpressionNode exp = ParseExpression();
            ExpectSymbol(')');
            ASTStatementNode statement = ParseStatement();
            return new ASTWhileNode(exp, statement);
        }

        private ASTDoWhileNode ParseDoWhile()
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

        private ASTBreakNode ParseBreak()
        {
            ExpectKeyword(Keyword.KeywordTypes.BREAK, out int line, out int column);
            ExpectSymbol(';');
            return new ASTBreakNode() { LineNumber = line, LineCharacter = column };
        }

        private ASTContinueNode ParseContinue()
        {
            ExpectKeyword(Keyword.KeywordTypes.CONTINUE, out int line, out int column);
            ExpectSymbol(';');
            return new ASTContinueNode() { LineNumber = line, LineCharacter = column };
        }

        private ASTForNode ParseFor()
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

        private ASTForDeclarationNode ParseForDeclaration()
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

        private ASTAbstractExpressionNode ParseOptionalExpression()
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

        private ASTStatementNode ParseStatement()
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

        private ASTConditionNode ParseCondition()
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

        private ASTDeclarationNode ParseDeclaration()
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

        private ASTReturnNode ParseReturn()
        {
            ExpectKeyword(Keyword.KeywordTypes.RETURN, out int line, out int column);
            ASTAbstractExpressionNode exp = ParseExpression();
            ExpectSymbol(';');
            return new ASTReturnNode(exp) { LineNumber = line, LineCharacter = column };
        }

        private ASTConstantNode ParseConstant()
        {
            ExpectInteger(out int value);
            return new ASTConstantNode(value) { LineNumber = currLine, LineCharacter = currColumn };
        }

        private ASTUnaryOpNode ParseUnaryOp()
        {
            ExpectUnarySymbol(out char symbol);
            ASTAbstractExpressionNode exp = ParseFactor();
            return new ASTUnaryOpNode(symbol, exp);
        }

        private ASTFunctionCallNode ParseFunctionCall()
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

        private ASTAbstractExpressionNode ParseFactor()
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

        private ASTAbstractExpressionNode ParseTerm()
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

        private ASTAbstractExpressionNode ParseAdditiveExpression()
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

        private ASTAbstractExpressionNode ParseShiftExpression()
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

        private ASTAbstractExpressionNode ParseRelationalExpression()
        {
            ASTAbstractExpressionNode exp = ParseShiftExpression();
            while (PeekSymbol('<') || PeekSymbol('>') ||
                   PeekSymbol2("<=") || PeekSymbol2(">="))
            {
                ExpectBinarySymbol(out string binOp);
                ASTAbstractExpressionNode second = ParseShiftExpression();
                exp = new ASTBinaryOpNode(binOp, exp, second) { IsComparison = true };
            }
            return exp;
        }

        private ASTAbstractExpressionNode ParseEqualityExpression()
        {
            ASTAbstractExpressionNode exp = ParseRelationalExpression();
            while (PeekSymbol2("!=") || PeekSymbol2("=="))
            {
                ExpectBinarySymbol(out string binOp);
                ASTAbstractExpressionNode second = ParseRelationalExpression();
                exp = new ASTBinaryOpNode(binOp, exp, second) { IsComparison = true };
            }
            return exp;
        }

        private ASTAbstractExpressionNode ParseBitwiseAndExpression()
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

        private ASTAbstractExpressionNode ParseBitwiseXorExpression()
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

        private ASTAbstractExpressionNode ParseBitwiseOrExpression()
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

        private ASTAbstractExpressionNode ParseLogicalAndExpression()
        {
            ASTAbstractExpressionNode exp = ParseBitwiseOrExpression();
            while (PeekSymbol2("&&"))
            {
                ExpectBinarySymbol(out string binOp);
                ASTAbstractExpressionNode second = ParseBitwiseOrExpression();
                exp = new ASTBinaryOpNode(binOp, exp, second) { NeedsShortCircuit = true };
            }
            return exp;
        }

        private ASTAbstractExpressionNode ParseLogicalOrExpression()
        {
            ASTAbstractExpressionNode exp = ParseLogicalAndExpression();
            while (PeekSymbol2("||"))
            {
                ExpectBinarySymbol(out string binOp);
                ASTAbstractExpressionNode second = ParseLogicalAndExpression();
                exp = new ASTBinaryOpNode(binOp, exp, second) { NeedsShortCircuit = true };
            }
            return exp;
        }

        private ASTAbstractExpressionNode ParseConditionalExpression()
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

        private ASTAbstractExpressionNode ParseExpression()
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

        private ASTAssignNode ParseAssignment()
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

        private bool HasMoreTokens()
        {
            return index < tokens.Count;
        }

        private Token Next()
        {
            if (!HasMoreTokens())
            {
                Fail("Missing Tokens");
            }

            currLine = tokens[index].Line;
            currColumn = tokens[index].Column;

            return tokens[index++];
        }

        private Token Peek()
        {
            if (!HasMoreTokens())
            {
                Fail("Missing Tokens");
            }

            return tokens[index];
        }

        private Token Peek(int forward)
        {
            if (!(index + forward < tokens.Count))
            {
                Fail("Missing Tokens");
            }

            return tokens[index + forward];
        }

        private void ExpectSymbol(char value)
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

        private bool PeekSymbol(char value)
        {
            return Peek() is Symbol symbol && symbol.Value == value;
        }

        private bool PeekUnarySymbol()
        {
            return Peek() is Symbol symbol && Symbol.Unary.Contains(symbol.Value);
        }

        private void ExpectSymbol2(string value)
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

        private bool PeekSymbol2(string value)
        {
            return Peek() is Symbol2 symbol2 && symbol2.Value == value;
        }

        private void ExpectKeyword(Keyword.KeywordTypes type)
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

        private void ExpectKeyword(Keyword.KeywordTypes type, out int line, out int column)
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

        private bool PeekKeyword(Keyword.KeywordTypes type)
        {
            return Peek() is Keyword keyword && keyword.KeywordType == type;
        }

        private void ExpectIdentifier(out string id)
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

        private void ExpectIdentifier(out string id, out int line, out int column)
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

        private void ExpectInteger(out int value)
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

        private void ExpectUnarySymbol(out char symbol)
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

        private void Fail(string message)
        {
            failed = true;
            if (index == tokens.Count) index--;
            throw new UnexpectedValueException("Fail: " + message + " at Line: " + tokens[index].Line + ", Column: " + tokens[index].Column);
        }

        private void Fail(Token.TokenType expected)
        {
            Fail("Expected " + expected);
        }

        private void Fail(Token.TokenType expected, string value)
        {
            Fail("Expected " + expected + " with Value '" + value + "'");
        }

        private bool Failed()
        {
            return failed;
        }
    }
}