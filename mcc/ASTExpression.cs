using System.Text;

namespace mcc
{
    class ASTExpression : ASTAbstractExpression
    {
        ASTAbstractExpression Expression;
        ASTIdentifier Identifier;

        public override void Parse(Parser parser)
        {
            if (parser.Peek().Type == Token.TokenType.IDENTIFIER && parser.Peek(1) is Symbol symbol && symbol.Value == '=')
            {
                // assignment
                Identifier = new ASTIdentifier();
                Identifier.Parse(parser);

                parser.ExpectSymbol('=');

                Expression = new ASTExpression();
                Expression.Parse(parser);
            }
            else
            {
                Expression = new ASTConditionalExpression();
                Expression.Parse(parser);
            }
        }

        public override void Print(int indent)
        {
            if (Identifier != null)
            {
                Console.WriteLine(new string(' ', indent) + "ASSIGN");
                Identifier.Print(indent + 3);
                Expression.Print(indent + 3);
            }
            else
            {
                Expression.Print(indent);
            }
        }

        public override void GenerateX86(Generator generator)
        {
            if (Identifier != null)
            {
                Expression.GenerateX86(generator);

                generator.AssignVariable(Identifier.Value);
            }
            else
            {
                Expression.GenerateX86(generator);
            }
        }
    }
}