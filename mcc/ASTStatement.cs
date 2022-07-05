using System.Text;

namespace mcc
{
    class ASTStatement : AST
    {
        public ASTExpression Expression;
        ASTIdentifier Identifier;
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
            else if (parser.PeekKeyword(Keyword.KeywordTypes.INT))
            {
                // declaration
                parser.ExpectKeyword(Keyword.KeywordTypes.INT);

                Identifier = new ASTIdentifier();
                Identifier.Parse(parser);

                if (parser.PeekSymbol('='))
                {
                    // assignment
                    parser.ExpectSymbol('=');

                    Expression = new ASTExpression();
                    Expression.Parse(parser);
                }

                parser.ExpectSymbol(';');
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
            if (Identifier != null)
            {
                Console.WriteLine(new string(' ', indent) + "DECLARE");
                Identifier.Print(indent + 3);

                if (Expression != null)
                {
                    Console.WriteLine(new string(' ', indent + 3) + "ASSIGN");
                    Expression.Print(indent + 6);
                }
            }
            else if (isReturn)
            {
                Console.WriteLine(new string(' ', indent) + "RETURN");
                Expression.Print(indent + 3);
            }
            else
            {
                Console.WriteLine(new string(' ', indent) + "EXPR");
                Expression.Print(indent + 3);
            }
        }

        public override void GenerateX86(StringBuilder stringBuilder)
        {
            if (Identifier != null)
            {

            }
            else if (isReturn)
            {
                Expression.GenerateX86(stringBuilder);
                stringBuilder.AppendLine("ret");
            }
            else
            {
                Expression.GenerateX86(stringBuilder);
            }
        }
    }
}