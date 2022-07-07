using System.Text;

namespace mcc
{
    class ASTStatement : AST
    {
        ASTAbstractExpression Expression;
        ASTStatement Statement, OptionalStatement;
        ASTDeclaration Declaration;
        ASTExpressionOptionalClosingParenthesis ForPostExpression;
        ASTExpressionOptionalSemicolon ForInit, ForCondition;
        List<ASTBlockItem> BlockItemList = new List<ASTBlockItem>();
        Keyword.KeywordTypes keyWord;

        public override void Parse(Parser parser)
        {
            if (parser.PeekKeyword(Keyword.KeywordTypes.RETURN))
            {
                keyWord = Keyword.KeywordTypes.RETURN;
                parser.ExpectKeyword(Keyword.KeywordTypes.RETURN);

                Expression = new ASTExpression();
                Expression.Parse(parser);

                parser.ExpectSymbol(';');
            }
            else if (parser.PeekKeyword(Keyword.KeywordTypes.IF))
            {
                keyWord = Keyword.KeywordTypes.IF;
                parser.ExpectKeyword(Keyword.KeywordTypes.IF);
                parser.ExpectSymbol('(');

                Expression = new ASTExpression();
                Expression.Parse(parser);

                parser.ExpectSymbol(')');

                Statement = new ASTStatement();
                Statement.Parse(parser);

                if (parser.PeekKeyword(Keyword.KeywordTypes.ELSE))
                {
                    parser.ExpectKeyword(Keyword.KeywordTypes.ELSE);

                    OptionalStatement = new ASTStatement();
                    OptionalStatement.Parse(parser);
                }
            }
            else if (parser.PeekSymbol('{'))
            {
                parser.ExpectSymbol('{');

                while (!parser.PeekSymbol('}'))
                {
                    ASTBlockItem blockItem = new ASTBlockItem();
                    blockItem.Parse(parser);
                    BlockItemList.Add(blockItem);
                }

                parser.ExpectSymbol('}');
            }
            else if (parser.PeekKeyword(Keyword.KeywordTypes.FOR))
            {
                keyWord = Keyword.KeywordTypes.FOR;
                parser.ExpectKeyword(Keyword.KeywordTypes.FOR);
                parser.ExpectSymbol('(');

                if (parser.PeekKeyword(Keyword.KeywordTypes.INT))
                {
                    Declaration = new ASTDeclaration();
                    Declaration.Parse(parser);
                    // note: declaration already contains semicolon
                }
                else
                {
                    ForInit = new ASTExpressionOptionalSemicolon();
                    ForInit.Parse(parser);
                    // note: this already contains semicolon
                }

                ForCondition = new ASTExpressionOptionalSemicolon();
                ForCondition.Parse(parser);
                // note: this already contains semicolon

                ForPostExpression = new ASTExpressionOptionalClosingParenthesis();
                ForPostExpression.Parse(parser);
                // note: this already contains closing parenthesis

                Statement = new ASTStatement();
                Statement.Parse(parser);
            }
            else if (parser.PeekKeyword(Keyword.KeywordTypes.WHILE))
            {
                keyWord = Keyword.KeywordTypes.WHILE;
                parser.ExpectKeyword(Keyword.KeywordTypes.WHILE);
                parser.ExpectSymbol('(');

                Expression = new ASTExpression();
                Expression.Parse(parser);

                parser.ExpectSymbol(')');

                Statement = new ASTStatement();
                Statement.Parse(parser);

            }
            else if (parser.PeekKeyword(Keyword.KeywordTypes.DO))
            {
                keyWord = Keyword.KeywordTypes.DO;
                parser.ExpectKeyword(Keyword.KeywordTypes.DO);

                Statement = new ASTStatement();
                Statement.Parse(parser);

                parser.ExpectKeyword(Keyword.KeywordTypes.WHILE);
                parser.ExpectSymbol('(');

                Expression = new ASTExpression();
                Expression.Parse(parser);

                parser.ExpectSymbol(')');
                parser.ExpectSymbol(';');
            }
            else if (parser.PeekKeyword(Keyword.KeywordTypes.BREAK))
            {
                keyWord = Keyword.KeywordTypes.BREAK;
                parser.ExpectKeyword(Keyword.KeywordTypes.BREAK);
                parser.ExpectSymbol(';');
            }
            else if (parser.PeekKeyword(Keyword.KeywordTypes.CONTINUE))
            {
                keyWord = Keyword.KeywordTypes.CONTINUE;
                parser.ExpectKeyword(Keyword.KeywordTypes.CONTINUE);
                parser.ExpectSymbol(';');
            }
            else
            {
                Expression = new ASTExpressionOptionalSemicolon();
                Expression.Parse(parser);
                // note: this already contains semicolon
            }
        }

        public override void Print(int indent)
        {
            if (keyWord == Keyword.KeywordTypes.IF)
            {
                Console.WriteLine(new string(' ', indent) + "IF");

                Expression.Print(indent + 3);

                Console.WriteLine(new string(' ', indent) + "THEN");

                Statement.Print(indent + 3);

                if (OptionalStatement != null)
                {
                    Console.WriteLine(new string(' ', indent) + "ELSE");

                    OptionalStatement.Print(indent + 3);
                }
            }
            else if (keyWord == Keyword.KeywordTypes.RETURN)
            {
                Console.WriteLine(new string(' ', indent) + "RETURN");
                Expression.Print(indent + 3);
            }
            else if (keyWord == Keyword.KeywordTypes.FOR)
            {
                Console.WriteLine(new string(' ', indent) + "FOR");
                Console.WriteLine(new string(' ', indent + 3) + "INITIAL");
                if (Declaration != null)
                {
                    Declaration.Print(indent + 6);
                }
                else
                {
                    ForInit.Print(indent + 6);
                }
                Console.WriteLine(new string(' ', indent + 3) + "CONDITION");

                ForCondition.Print(indent + 6);

                Console.WriteLine(new string(' ', indent + 3) + "POST");

                ForPostExpression.Print(indent + 6);

                Console.WriteLine(new string(' ', indent) + "DO");
                Statement.Print(indent + 3);
            }
            else if (keyWord == Keyword.KeywordTypes.WHILE)
            {
                Console.WriteLine(new string(' ', indent) + "WHILE");
                Expression.Print(indent + 3);
                Console.WriteLine(new string(' ', indent) + "DO");
                Statement.Print(indent + 3);
            }
            else if (keyWord == Keyword.KeywordTypes.DO)
            {
                Console.WriteLine(new string(' ', indent) + "DO");
                Statement.Print(indent + 3);
                Console.WriteLine(new string(' ', indent) + "WHILE");
                Expression.Print(indent + 3);
            }
            else if (keyWord == Keyword.KeywordTypes.BREAK)
            {
                Console.WriteLine(new string(' ', indent) + "BREAK");
            }
            else if (keyWord == Keyword.KeywordTypes.CONTINUE)
            {
                Console.WriteLine(new string(' ', indent) + "CONTINUE");
            }
            else if (BlockItemList.Count > 0)
            {
                Console.WriteLine(new string(' ', indent) + "BLK_BEGIN");
                foreach (var statement in BlockItemList)
                    statement.Print(indent + 3);
                Console.WriteLine(new string(' ', indent) + "BLK_END");
            }
            else
            {
                Console.WriteLine(new string(' ', indent) + "EXPR");
                Expression.Print(indent + 3);
            }
        }

        public override void GenerateX86(Generator generator)
        {
            if (keyWord == Keyword.KeywordTypes.IF)
            {
                Expression.GenerateX86(generator);
                generator.CompareZero();
                string label = generator.JumpEqual();

                if (OptionalStatement != null)
                {
                    // with else
                    Statement.GenerateX86(generator);
                    string end = generator.Jump();
                    generator.Label(label);
                    OptionalStatement.GenerateX86(generator);
                    generator.Label(end);
                }
                else
                {
                    // without else
                    Statement.GenerateX86(generator);
                    generator.Label(label);
                }
            }
            else if (keyWord == Keyword.KeywordTypes.RETURN)
            {
                Expression.GenerateX86(generator);
                generator.FunctionEpilogue();
            }
            else if (keyWord == Keyword.KeywordTypes.FOR)
            {
                generator.BeginBlock();

                // init
                if (Declaration != null)
                {
                    Declaration.GenerateX86(generator);
                }
                else
                {
                    ForInit.GenerateX86(generator);
                }

                int loopCount = generator.LoopBeginLabel();

                if (ForCondition.Expression != null)
                    ForCondition.GenerateX86(generator);
                else
                    generator.IntegerConstant(0); // non zero value if condition is empty; todo: change to 1, 0 to not loop in test since break not implemented yet
                generator.CompareZero();
                generator.LoopJumpEqualEnd(loopCount);
                generator.BeginLoopBlock();
                Statement.GenerateX86(generator);
                generator.LoopContinueLabel(loopCount);
                generator.EndLoopBlock();
                ForPostExpression.GenerateX86(generator);
                generator.LoopJumpBegin(loopCount);
                generator.LoopEndLabel(loopCount);

                generator.EndBlock();
            }
            else if (keyWord == Keyword.KeywordTypes.WHILE)
            {
                int loopCount = generator.LoopBeginLabel();
                Expression.GenerateX86(generator);
                generator.CompareZero();
                generator.LoopJumpEqualEnd(loopCount);
                generator.BeginLoopBlock();
                Statement.GenerateX86(generator);
                generator.LoopContinueLabel(loopCount);
                generator.EndLoopBlock();
                generator.LoopJumpBegin(loopCount);
                generator.LoopEndLabel(loopCount);
            }
            else if (keyWord == Keyword.KeywordTypes.DO)
            {
                int loopCount = generator.LoopBeginLabel();
                generator.BeginLoopBlock();
                Statement.GenerateX86(generator);
                generator.LoopContinueLabel(loopCount);
                generator.EndLoopBlock();
                Expression.GenerateX86(generator);
                generator.CompareZero();
                generator.LoopJumpNotEqualBegin(loopCount);
                generator.LoopEndLabel(loopCount);
            }
            else if (keyWord == Keyword.KeywordTypes.BREAK)
            {
                generator.LoopBreak();
            }
            else if (keyWord == Keyword.KeywordTypes.CONTINUE)
            {
                generator.LoopContinue();
            }
            else if (BlockItemList.Count > 0)
            {
                generator.BeginBlock();

                foreach (var statement in BlockItemList)
                    statement.GenerateX86(generator);

                generator.EndBlock();
            }
            else
            {
                Expression.GenerateX86(generator);
            }
        }
    }
}