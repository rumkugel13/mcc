using System.Text;

namespace mcc
{
    class ASTFactor : ASTAbstractExpression
    {
        ASTExpression Expression;
        ASTUnaryOperation UnaryOperation;
        ASTInteger Integer;
        ASTIdentifier Identifier;
        ASTFunctionCall FunctionCall;

        public override void Parse(Parser parser)
        {
            if (parser.PeekSymbol('('))
            {
                parser.ExpectSymbol('(');

                Expression = new ASTExpression();
                Expression.Parse(parser);

                parser.ExpectSymbol(')');
            }
            else if (parser.PeekUnarySymbol())
            {
                UnaryOperation = new ASTUnaryOperation();
                UnaryOperation.Parse(parser);
            }
            else if (parser.Peek() is Integer)
            {
                Integer = new ASTInteger();
                Integer.Parse(parser);
            }
            else if (parser.Peek() is Identifier)
            {
                if (parser.Peek(1) is Symbol s && s.Value == '(')
                {
                    FunctionCall = new ASTFunctionCall();
                    FunctionCall.Parse(parser);
                }
                else
                {
                    Identifier = new ASTIdentifier();
                    Identifier.Parse(parser);
                }
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
            else if (FunctionCall != null)
            {
                FunctionCall.Print(indent);
            }
            else if (Identifier != null)
            {
                Identifier.Print(indent);
            }
        }

        public override void GenerateX86(Generator generator)
        {
            if (Expression != null)
            {
                Expression.GenerateX86(generator);
            }
            else if (UnaryOperation != null)
            {
                UnaryOperation.GenerateX86(generator);
            }
            else if (Integer != null)
            {
                Integer.GenerateX86(generator);
            }
            else if (FunctionCall != null)
            {
                FunctionCall.GenerateX86(generator);
            }
            else if (Identifier != null)
            {
                generator.ReferenceVariable(Identifier.Value);
            }
        }
    }
}