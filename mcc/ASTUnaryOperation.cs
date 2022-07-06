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
                parser.Fail(Token.TokenType.SYMBOL, "'-' or '~' or '!'");

            Value = (token as Symbol).Value;

            Factor.Parse(parser);
        }

        public override void Print(int indent)
        {
            Console.WriteLine(new String(' ', indent) + "UnOP<" + Value + ">");
            Factor.Print(indent + 3);
        }

        public override void GenerateX86(StringBuilder stringBuilder)
        {
            Factor.GenerateX86(stringBuilder);

            switch (Value)
            {
                case '-': stringBuilder.AppendLine("negq %rax"); break;
                case '~': stringBuilder.AppendLine("notq %rax"); break;
                case '!':
                    stringBuilder.AppendLine("cmpq $0, %rax");
                    stringBuilder.AppendLine("movq $0, %rax");
                    stringBuilder.AppendLine("sete %al");
                    break;
            }
        }

        public override void GenerateX86(Generator generator)
        {
            Factor.GenerateX86(generator);

            switch (Value)
            {
                case '-': generator.Instruction("negq %rax"); break;
                case '~': generator.Instruction("notq %rax"); break;
                case '!':
                    generator.Instruction("cmpq $0, %rax");
                    generator.Instruction("movq $0, %rax");
                    generator.Instruction("sete %al");
                    break;
            }
        }
    }
}