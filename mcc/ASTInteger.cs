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
            parser.ExpectInteger(out int value);

            Value = value;
        }

        public override void Print(int indent)
        {
            Console.WriteLine(new String(' ', indent) + "INT<" + Value + ">");
        }

        public override void GenerateX86(Generator generator)
        {
            generator.IntegerConstant(Value);
        }
    }
}