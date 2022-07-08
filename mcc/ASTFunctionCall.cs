using System.Text;

namespace mcc
{
    class ASTFunctionCall : AST
    {
        ASTIdentifier Identifier;
        List<ASTExpression> Arguments = new List<ASTExpression>();

        public override void Parse(Parser parser)
        {
            Identifier = new ASTIdentifier();
            Identifier.Parse(parser);

            parser.ExpectSymbol('(');

            while (!parser.PeekSymbol(')'))
            {
                ASTExpression expression = new ASTExpression();
                expression.Parse(parser);
                Arguments.Add(expression);

                if (!parser.PeekSymbol(','))
                {
                    break;
                }
                else
                {
                    parser.ExpectSymbol(',');
                }
            }

            parser.ExpectSymbol(')');
        }

        public override void Print(int indent)
        {
            Console.WriteLine(new string(' ', indent) + "CALL");
            Identifier.Print(indent + 3);
            Console.WriteLine(new string(' ', indent) + "ARGS_BEGIN");
            foreach (var argument in Arguments)
            {
                argument.Print(indent + 3);
            }
            Console.WriteLine(new string(' ', indent) + "ARGS_END");
        }

        public override void GenerateX86(Generator generator)
        {
            
        }
    }
}