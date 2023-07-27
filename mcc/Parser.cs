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
            while(HasMoreTokens() && Peek() is not EndToken)
            {
                ASTTopLevelItemNode topLevelItem = ParseTopLevelItem();
                topLevelItems.Add(topLevelItem);
            }

            return new ASTProgramNode(programName, topLevelItems);
        }

        private ASTTopLevelItemNode ParseTopLevelItem()
        {
            if (Peek(2) is Symbol symbol && symbol.SymbolType == Symbol.SymbolTypes.OPEN_PARENTHESIS)
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
            ExpectSymbol(Symbol.SymbolTypes.OPEN_PARENTHESIS);

            List<string> parameters = new List<string>();
            if (PeekKeyword(Keyword.KeywordTypes.INT))
            {
                ExpectKeyword(Keyword.KeywordTypes.INT);
                ExpectIdentifier(out string id);
                parameters.Add(id);

                while (PeekSymbol(Symbol.SymbolTypes.COMMA))
                {
                    ExpectSymbol(Symbol.SymbolTypes.COMMA);
                    ExpectKeyword(Keyword.KeywordTypes.INT);
                    ExpectIdentifier(out string id2);
                    parameters.Add(id2);
                }
            }

            ExpectSymbol(Symbol.SymbolTypes.CLOSE_PARENTHESIS);

            if (PeekSymbol(Symbol.SymbolTypes.OPEN_BRACES))
            {
                List<ASTBlockItemNode> blockItems = new List<ASTBlockItemNode>();
                ExpectSymbol(Symbol.SymbolTypes.OPEN_BRACES);
                while (!PeekSymbol(Symbol.SymbolTypes.CLOSE_BRACES))
                {
                    blockItems.Add(ParseBlockItem());
                }
                ExpectSymbol(Symbol.SymbolTypes.CLOSE_BRACES);
                return new ASTFunctionNode(name, parameters, blockItems) { LineNumber = line, LineCharacter = column};
            }
            else
            {
                ExpectSymbol(Symbol.SymbolTypes.SEMICOLON);
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
            ExpectSymbol(Symbol.SymbolTypes.OPEN_BRACES);

            List<ASTBlockItemNode> blockItems = new List<ASTBlockItemNode>();
            while (!PeekSymbol(Symbol.SymbolTypes.CLOSE_BRACES))
            {
                blockItems.Add(ParseBlockItem());
            }
            ExpectSymbol(Symbol.SymbolTypes.CLOSE_BRACES);
            return new ASTCompundNode(blockItems);
        }

        private ASTWhileNode ParseWhile()
        {
            ExpectKeyword(Keyword.KeywordTypes.WHILE);
            ExpectSymbol(Symbol.SymbolTypes.OPEN_PARENTHESIS);
            ASTAbstractExpressionNode exp = ParseExpression();
            ExpectSymbol(Symbol.SymbolTypes.CLOSE_PARENTHESIS);
            ASTStatementNode statement = ParseStatement();
            return new ASTWhileNode(exp, statement);
        }

        private ASTDoWhileNode ParseDoWhile()
        {
            ExpectKeyword(Keyword.KeywordTypes.DO);
            ASTStatementNode statement = ParseStatement();
            ExpectKeyword(Keyword.KeywordTypes.WHILE);
            ExpectSymbol(Symbol.SymbolTypes.OPEN_PARENTHESIS);
            ASTAbstractExpressionNode exp = ParseExpression();
            ExpectSymbol(Symbol.SymbolTypes.CLOSE_PARENTHESIS);
            ExpectSymbol(Symbol.SymbolTypes.SEMICOLON);
            return new ASTDoWhileNode(statement, exp);
        }

        private ASTBreakNode ParseBreak()
        {
            ExpectKeyword(Keyword.KeywordTypes.BREAK, out int line, out int column);
            ExpectSymbol(Symbol.SymbolTypes.SEMICOLON);
            return new ASTBreakNode() { LineNumber = line, LineCharacter = column };
        }

        private ASTContinueNode ParseContinue()
        {
            ExpectKeyword(Keyword.KeywordTypes.CONTINUE, out int line, out int column);
            ExpectSymbol(Symbol.SymbolTypes.SEMICOLON);
            return new ASTContinueNode() { LineNumber = line, LineCharacter = column };
        }

        private ASTForNode ParseFor()
        {
            ExpectKeyword(Keyword.KeywordTypes.FOR);
            ExpectSymbol(Symbol.SymbolTypes.OPEN_PARENTHESIS);
            ASTAbstractExpressionNode init = ParseOptionalExpression();
            ExpectSymbol(Symbol.SymbolTypes.SEMICOLON);
            ASTAbstractExpressionNode condition = ParseOptionalExpression();
            if (condition is ASTNoExpressionNode)
            {
                condition = new ASTConstantNode(1); // insert a true condition
            }

            ExpectSymbol(Symbol.SymbolTypes.SEMICOLON);
            ASTAbstractExpressionNode post = ParseOptionalExpression();
            ExpectSymbol(Symbol.SymbolTypes.CLOSE_PARENTHESIS);
            ASTStatementNode statement = ParseStatement();
            return new ASTForNode(statement, init, condition, post);
        }

        private ASTForDeclarationNode ParseForDeclaration()
        {
            ExpectKeyword(Keyword.KeywordTypes.FOR);
            ExpectSymbol(Symbol.SymbolTypes.OPEN_PARENTHESIS);
            ASTDeclarationNode decl = ParseDeclaration(); // includes ;
            ASTAbstractExpressionNode condition = ParseOptionalExpression();
            if (condition is ASTNoExpressionNode)
            {
                condition = new ASTConstantNode(1); // insert a true condition
            }

            ExpectSymbol(Symbol.SymbolTypes.SEMICOLON);
            ASTAbstractExpressionNode post = ParseOptionalExpression();
            ExpectSymbol(Symbol.SymbolTypes.CLOSE_PARENTHESIS);
            ASTStatementNode statement = ParseStatement();
            return new ASTForDeclarationNode(statement, decl, condition, post);
        }

        private ASTAbstractExpressionNode ParseOptionalExpression(bool isStatement = false)
        {
            if (!PeekSymbol(Symbol.SymbolTypes.SEMICOLON) && !PeekSymbol(Symbol.SymbolTypes.CLOSE_PARENTHESIS))
            {
                return ParseExpression(isStatement);
            }
            else
            {
                return new ASTNoExpressionNode();
            }
        }

        private ASTStatementNode ParseStatement()
        {
            if (Peek() is Keyword keyword)
            {
                switch (keyword.KeywordType)
                {
                    case Keyword.KeywordTypes.RETURN: return ParseReturn();
                    case Keyword.KeywordTypes.IF: return ParseCondition();
                    case Keyword.KeywordTypes.FOR:
                        return Peek(2) is Keyword kw && kw.KeywordType == Keyword.KeywordTypes.INT ? ParseForDeclaration() : ParseFor();
                    case Keyword.KeywordTypes.WHILE: return ParseWhile();
                    case Keyword.KeywordTypes.DO: return ParseDoWhile();
                    case Keyword.KeywordTypes.BREAK: return ParseBreak();
                    case Keyword.KeywordTypes.CONTINUE: return ParseContinue();
                    default: Fail("Unexpected keyword " + keyword.KeywordType); return new ASTNoStatementNode();
                }
            }
            else if (PeekSymbol(Symbol.SymbolTypes.OPEN_BRACES))
            {
                return ParseCompound();
            }
            else
            {
                ASTAbstractExpressionNode exp = ParseOptionalExpression(true);
                ExpectSymbol(Symbol.SymbolTypes.SEMICOLON);
                return new ASTExpressionNode(exp) { LineNumber = exp.LineNumber, LineCharacter = exp.LineCharacter };
            }
        }

        private ASTConditionNode ParseCondition()
        {
            ExpectKeyword(Keyword.KeywordTypes.IF);
            ExpectSymbol(Symbol.SymbolTypes.OPEN_PARENTHESIS);
            ASTAbstractExpressionNode condition = ParseExpression();
            ExpectSymbol(Symbol.SymbolTypes.CLOSE_PARENTHESIS);
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

            if (PeekSymbol(Symbol.SymbolTypes.EQUALS))
            {
                ExpectSymbol(Symbol.SymbolTypes.EQUALS);
                ASTAbstractExpressionNode exp = ParseExpression();
                ExpectSymbol(Symbol.SymbolTypes.SEMICOLON);
                return new ASTDeclarationNode(id, exp) { LineNumber = line, LineCharacter = column };
            }
            else
            {
                ExpectSymbol(Symbol.SymbolTypes.SEMICOLON);
                return new ASTDeclarationNode(id) { LineNumber = line, LineCharacter = column };
            }
        }

        private ASTReturnNode ParseReturn()
        {
            ExpectKeyword(Keyword.KeywordTypes.RETURN, out int line, out int column);
            ASTAbstractExpressionNode exp = ParseExpression();
            ExpectSymbol(Symbol.SymbolTypes.SEMICOLON);
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
            ExpectSymbol(Symbol.SymbolTypes.OPEN_PARENTHESIS);

            List<ASTAbstractExpressionNode> arguments = new List<ASTAbstractExpressionNode>();
            if (!PeekSymbol(Symbol.SymbolTypes.CLOSE_PARENTHESIS))
            {
                arguments.Add(ParseExpression());
                while (PeekSymbol(Symbol.SymbolTypes.COMMA))
                {
                    ExpectSymbol(Symbol.SymbolTypes.COMMA);
                    arguments.Add(ParseExpression());
                }
            }

            ExpectSymbol(Symbol.SymbolTypes.CLOSE_PARENTHESIS);
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
            else if (PeekSymbol(Symbol.SymbolTypes.OPEN_PARENTHESIS))
            {
                ExpectSymbol(Symbol.SymbolTypes.OPEN_PARENTHESIS);
                ASTAbstractExpressionNode exp = ParseExpression();
                ExpectSymbol(Symbol.SymbolTypes.CLOSE_PARENTHESIS);
                return exp;
            }
            else if (Peek() is Identifier)
            {
                if (Peek(1) is Symbol symbol && symbol.SymbolType == Symbol.SymbolTypes.OPEN_PARENTHESIS)
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
            while (PeekSymbol(Symbol.SymbolTypes.MULTIPLICATION) || PeekSymbol(Symbol.SymbolTypes.DIVISION) || PeekSymbol(Symbol.SymbolTypes.REMAINDER))
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
            while (PeekSymbol(Symbol.SymbolTypes.PLUS) || PeekSymbol(Symbol.SymbolTypes.MINUS))
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
            while (PeekSymbol(Symbol.SymbolTypes.SHIFT_LEFT) || PeekSymbol(Symbol.SymbolTypes.SHIFT_RIGHT))
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
            while (PeekSymbol(Symbol.SymbolTypes.LESS_THAN) || PeekSymbol(Symbol.SymbolTypes.GREATER_THAN) ||
                   PeekSymbol(Symbol.SymbolTypes.LESS_EQUAL) || PeekSymbol(Symbol.SymbolTypes.GREATER_EQUAL))
            {
                ExpectBinarySymbol(out string binOp);
                ASTAbstractExpressionNode second = ParseShiftExpression();
                exp = new ASTBinaryOpNode(binOp, exp, second);
            }
            return exp;
        }

        private ASTAbstractExpressionNode ParseEqualityExpression()
        {
            ASTAbstractExpressionNode exp = ParseRelationalExpression();
            while (PeekSymbol(Symbol.SymbolTypes.NOT_EQUALS) || PeekSymbol(Symbol.SymbolTypes.DOUBLE_EQUALS))
            {
                ExpectBinarySymbol(out string binOp);
                ASTAbstractExpressionNode second = ParseRelationalExpression();
                exp = new ASTBinaryOpNode(binOp, exp, second);
            }
            return exp;
        }

        private ASTAbstractExpressionNode ParseBitwiseAndExpression()
        {
            ASTAbstractExpressionNode exp = ParseEqualityExpression();
            while (PeekSymbol(Symbol.SymbolTypes.BIT_AND))
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
            while (PeekSymbol(Symbol.SymbolTypes.BIT_XOR))
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
            while (PeekSymbol(Symbol.SymbolTypes.BIT_OR))
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
            while (PeekSymbol(Symbol.SymbolTypes.LOGICAL_AND))
            {
                ExpectBinarySymbol(out string binOp);
                ASTAbstractExpressionNode second = ParseBitwiseOrExpression();
                exp = new ASTBinaryOpNode(binOp, exp, second);
            }
            return exp;
        }

        private ASTAbstractExpressionNode ParseLogicalOrExpression()
        {
            ASTAbstractExpressionNode exp = ParseLogicalAndExpression();
            while (PeekSymbol(Symbol.SymbolTypes.LOGICAL_OR))
            {
                ExpectBinarySymbol(out string binOp);
                ASTAbstractExpressionNode second = ParseLogicalAndExpression();
                exp = new ASTBinaryOpNode(binOp, exp, second);
            }
            return exp;
        }

        private ASTAbstractExpressionNode ParseConditionalExpression()
        {
            ASTAbstractExpressionNode exp = ParseLogicalOrExpression();
            if (PeekSymbol(Symbol.SymbolTypes.QUESTION))
            {
                ExpectSymbol(Symbol.SymbolTypes.QUESTION);
                ASTAbstractExpressionNode ifBranch = ParseExpression();
                ExpectSymbol(Symbol.SymbolTypes.COLON);
                ASTAbstractExpressionNode elseBranch = ParseConditionalExpression();
                return new ASTConditionalExpressionNode(exp, ifBranch, elseBranch);
            }
            else
            {
                return exp;
            }
        }

        private ASTAbstractExpressionNode ParseExpression(bool isStatement = false)
        {
            if (Peek() is Identifier && Peek(1) is Symbol symbol && symbol.SymbolType == Symbol.SymbolTypes.EQUALS)
            {
                var ass = ParseAssignment();
                ass.IsStatement = isStatement;
                return ass;
            }
            else
            {
                var exp = ParseConditionalExpression();
                exp.IsStatement = isStatement;
                return exp;
            }
        }

        private ASTAssignNode ParseAssignment()
        {
            ExpectIdentifier(out string id, out int line, out int column);
            ExpectSymbol(Symbol.SymbolTypes.EQUALS);
            ASTAbstractExpressionNode expression = ParseExpression();
            return new ASTAssignNode(id, expression) { LineNumber = line, LineCharacter = column };
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

            currLine = tokens[index].Position.Line;
            currColumn = tokens[index].Position.Column;

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

        private bool PeekSymbol(Symbol.SymbolTypes type)
        {
            return Peek() is Symbol symbol && symbol.SymbolType == type;
        }

        private void ExpectSymbol(Symbol.SymbolTypes value)
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

        private bool PeekUnarySymbol()
        {
            return Peek() is Symbol symbol && Symbol.IsUnary(symbol.SymbolType);
        }

        private void ExpectUnarySymbol(out char symbol)
        {
            symbol = char.MinValue;
            if (PeekUnarySymbol())
            {
                symbol = ((Symbol)Next()).Value[0];
            }
            else
            {
                Fail("Expected Unary Operator");
            }
        }

        private bool PeekBinarySymbol()
        {
            return Peek() is Symbol symbol && Symbol.IsBinary(symbol.SymbolType);
        }

        private void ExpectBinarySymbol(out string value)
        {
            value = "";
            if (PeekBinarySymbol())
            {
                value = ((Symbol)Next()).Value;
            }
            else
            {
                Fail("Expected Binary Operator");
            }
        }

        private bool PeekKeyword(Keyword.KeywordTypes type)
        {
            return Peek() is Keyword keyword && keyword.KeywordType == type;
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

        private void Fail(string message)
        {
            failed = true;
            if (index == tokens.Count) index--;
            throw new UnexpectedValueException("Fail: " + message + " at " + tokens[index].Position);
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