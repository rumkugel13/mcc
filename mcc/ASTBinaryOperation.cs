using System.Text;

namespace mcc
{
    class ASTBinaryOperation : AST
    {
        public char Value;

        public override void Parse(Parser parser)
        {
            Token token = parser.Next();
            if (token is not Symbol && Symbol.Binary.Contains((token as Symbol).Value))
                parser.Fail(Token.TokenType.SYMBOL);

            Value = (token as Symbol).Value;
        }

        public override void Print(int indent)
        {
            Console.WriteLine(new String(' ', indent) + "Binary<" + Value + ">");
        }

        public override void GenerateX86(StringBuilder stringBuilder)
        {
            switch (Value)
            {
                case '+':
                    stringBuilder.AppendLine("addq %rcx, %rax");
                    break;
                case '*':
                    stringBuilder.AppendLine("imulq %rcx, %rax");
                    break;
                case '-':
                    stringBuilder.AppendLine("subq %rax, %rcx");
                    stringBuilder.AppendLine("movq %rcx, %rax");
                    break;
                case '/':
                    stringBuilder.AppendLine("cdq");
                    stringBuilder.AppendLine("idivl %ecx");
                    break;
                default:
                    break;
            }
        }
    }
}