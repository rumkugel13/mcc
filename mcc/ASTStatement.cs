using System.Text;

namespace mcc
{
    class ASTStatement : AST
    {
        ASTExpression Expression;
        ASTStatement Statement, OptionalStatement;
        List<ASTBlockItem> BlockItemList = new List<ASTBlockItem>();
        bool isReturn = false;

        public override void Parse(Parser parser)
        {
            if (parser.PeekKeyword(Keyword.KeywordTypes.RETURN))
            {
                isReturn = true;
                parser.ExpectKeyword(Keyword.KeywordTypes.RETURN);

                Expression = new ASTExpression();
                Expression.Parse(parser);

                parser.ExpectSymbol(';');
            }
            else if (parser.PeekKeyword(Keyword.KeywordTypes.IF))
            {
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
            else
            {
                Expression = new ASTExpression();
                Expression.Parse(parser);

                parser.ExpectSymbol(';');
            }
        }

        public override void Print(int indent)
        {
            if (Statement != null)
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
            else if (isReturn)
            {
                Console.WriteLine(new string(' ', indent) + "RETURN");
                Expression.Print(indent + 3);
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
            if (Statement != null)
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
            else if (isReturn)
            {
                Expression.GenerateX86(generator);
                generator.FunctionEpilogue();
            }
            else if (BlockItemList.Count > 0)
            {
                foreach (var statement in BlockItemList)
                    statement.GenerateX86(generator);
            }
            else
            {
                Expression.GenerateX86(generator);
            }
        }
    }
}