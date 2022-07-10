using System.Text;

namespace mcc
{
    class ASTExpression : AST
    {
        ASTExpression Expression;
        ASTIdentifier Identifier;
        ASTConditionalExpression ConditionalExpression;

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
                ConditionalExpression = new ASTConditionalExpression();
                ConditionalExpression.Parse(parser);
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
                ConditionalExpression.Print(indent);
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
                ConditionalExpression.GenerateX86(generator);
            }
        }
    }
}