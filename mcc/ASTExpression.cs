using System.Text;

namespace mcc
{
    class ASTExpression : ASTAbstractExpression
    {
        ASTAbstractExpression Expression;
        ASTIdentifier Identifier;

        public override void Parse(Parser parser)
        {
            if (parser.Peek().Type == Token.TokenType.IDENTIFIER && parser.PeekNext() is Symbol symbol && symbol.Value == '=')
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
                Expression = new ASTLogicalOrExpression();
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

        public override void GenerateX86(StringBuilder stringBuilder)
        {
            if (Identifier != null)
            {
                // assign variable
                Expression.GenerateX86(stringBuilder);

                if (VariableMap.TryGetValue(Identifier.Value, out int offset))
                {
                    stringBuilder.AppendLine("movq %rax, " + offset + "(%rbp)");
                }
                else
                    throw new InvalidOperationException("Trying to assign to non existing Variable: " + Identifier.Value);
            }
            else
            {
                Expression.GenerateX86(stringBuilder);
            }
        }
    }
}