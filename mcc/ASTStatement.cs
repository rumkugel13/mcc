using System.Text;

namespace mcc
{
    class ASTStatement : AST
    {
        ASTAbstractExpression Expression;
        ASTStatement Statement, OptionalStatement;
        ASTDeclaration Declaration;
        ASTExpressionOptionalClosingParenthesis ExpressionOptionalClosingParenthesis;
        ASTExpressionOptionalSemicolon ExpressionOptionalSemicolon, ExpressionOptionalSemicolon2;
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
                    ExpressionOptionalSemicolon = new ASTExpressionOptionalSemicolon();
                    ExpressionOptionalSemicolon.Parse(parser);
                    // note: this already contains semicolon
                }

                ExpressionOptionalSemicolon2 = new ASTExpressionOptionalSemicolon();
                ExpressionOptionalSemicolon2.Parse(parser);
                // note: this already contains semicolon

                ExpressionOptionalClosingParenthesis = new ASTExpressionOptionalClosingParenthesis();
                ExpressionOptionalClosingParenthesis.Parse(parser);
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
                    ExpressionOptionalSemicolon.Print(indent + 6);
                }
                Console.WriteLine(new string(' ', indent + 3) + "CONDITION");

                ExpressionOptionalSemicolon2.Print(indent + 6);

                Console.WriteLine(new string(' ', indent + 3) + "POST");

                ExpressionOptionalClosingParenthesis.Print(indent + 6);

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
                Console.WriteLine(new string(' ', indent) + "BLK_START");
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

            }
            else if (keyWord == Keyword.KeywordTypes.WHILE)
            {

            }
            else if (keyWord == Keyword.KeywordTypes.DO)
            {

            }
            else if (keyWord == Keyword.KeywordTypes.BREAK)
            {

            }
            else if (keyWord == Keyword.KeywordTypes.CONTINUE)
            {

            }
            else if (BlockItemList.Count > 0)
            {
                generator.StartBlock();

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