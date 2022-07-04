using System.Text;

namespace mcc
{
    class ASTUnaryOperation : AST
    {
        public ASTFactor Factor;
        public char Value;

        public ASTUnaryOperation()
        {
            Factor = new ASTFactor();
        }

        public override void Parse(Parser parser)
        {
            Token token = parser.Next();
            if (token is not Symbol && Symbol.Unary.Contains((token as Symbol).Value))
                parser.Fail(Token.TokenType.SYMBOL);

            Value = (token as Symbol).Value;

            Factor.Parse(parser);
        }

        public override void Print(int indent)
        {
            Console.WriteLine(new String(' ', indent) + "Unary<" + Value + ">");
            Factor.Print(indent + 3);
        }

        public override void GenerateX86(StringBuilder stringBuilder)
        {
            switch (Value)
            {
                case '-':
                    Factor.GenerateX86(stringBuilder);
                    stringBuilder.AppendLine("negq %rax");
                    break;
                case '~':
                    Factor.GenerateX86(stringBuilder);
                    stringBuilder.AppendLine("notq %rax");
                    break;
                case '!':
                    Factor.GenerateX86(stringBuilder);
                    stringBuilder.AppendLine("cmpq $0, %rax");
                    stringBuilder.AppendLine("movq $0, %rax");
                    stringBuilder.AppendLine("sete %al");
                    break;
                default:
                    break;
            }
        }
    }
}