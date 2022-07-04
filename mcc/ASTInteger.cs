using System.Text;

namespace mcc
{
    class ASTInteger : AST
    {
        public int Value;

        public ASTInteger()
        {
            Value = 0;
        }

        public override void Parse(Parser parser)
        {
            Token token = parser.Next();
            if (token is not Integer)
                parser.Fail(Token.TokenType.INTEGER);

            Value = (token as Integer).Value;
        }

        public override void Print(int indent)
        {
            Console.WriteLine(new String(' ', indent) + "INT<" + Value + ">");
        }

        public override void GenerateX86(StringBuilder stringBuilder)
        {
            stringBuilder.AppendLine("movq $" + Value + ", %rax");
        }
    }
}