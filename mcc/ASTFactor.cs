using System.Text;

namespace mcc
{
    class ASTFactor : ASTAbstractExpression
    {
        ASTExpression Expression;
        ASTUnaryOperation UnaryOperation;
        ASTInteger Integer;
        ASTIdentifier Identifier;

        public override void Parse(Parser parser)
        {
            Token peek = parser.Peek();

            if (peek is Symbol symbol && symbol.Value == '(')
            {
                Token next = parser.Next();
                if (next is not Symbol || (next as Symbol).Value != '(')
                    parser.Fail(Token.TokenType.SYMBOL, "(");

                Expression = new ASTExpression();
                Expression.Parse(parser);

                next = parser.Next();
                if (next is not Symbol || (next as Symbol).Value != ')')
                    parser.Fail(Token.TokenType.SYMBOL, ")");
            }
            else if (peek is Symbol && Symbol.Unary.Contains((peek as Symbol).Value))
            {
                UnaryOperation = new ASTUnaryOperation();
                UnaryOperation.Parse(parser);
            }
            else if (peek is Integer)
            {
                Integer = new ASTInteger();
                Integer.Parse(parser);
            }
            else if (peek is Identifier)
            {
                Identifier = new ASTIdentifier();
                Identifier.Parse(parser);
            }
            else
            {
                parser.Fail(Token.TokenType.SYMBOL, "'(' or Unary or INT");
            }
        }

        public override void Print(int indent)
        {
            if (Expression != null)
            {
                Console.WriteLine(new string(' ', indent) + "(");
                Expression.Print(indent);
                Console.WriteLine(new string(' ', indent) + ")");
            }
            else if (UnaryOperation != null)
            {
                UnaryOperation.Print(indent);
            }
            else if (Integer != null)
            {
                Integer.Print(indent);
            }
            else if (Identifier != null)
            {
                Identifier.Print(indent);
            }
        }

        public override void GenerateX86(StringBuilder stringBuilder)
        {
            if (Expression != null)
            {
                Expression.GenerateX86(stringBuilder);
            }
            else if (UnaryOperation != null)
            {
                UnaryOperation.GenerateX86(stringBuilder);
            }
            else if (Integer != null)
            {
                Integer.GenerateX86(stringBuilder);
            }
            else if (Identifier != null)
            {
                Identifier.GenerateX86(stringBuilder);
            }
        }
    }
}