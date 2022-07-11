using System.Text;

namespace mcc
{
    class ASTUnaryOperation : AST
    {
        ASTFactor Factor;
        char Value;

        public ASTUnaryOperation()
        {
            Factor = new ASTFactor();
        }

        public override void Parse(Parser parser)
        {
            parser.ExpectUnarySymbol(out char value);

            Value = value;

            Factor.Parse(parser);
        }

        public override void Print(int indent)
        {
            Console.WriteLine(new String(' ', indent) + "UnOP<" + Value + ">");
            Factor.Print(indent + 3);
        }

        public override void GenerateX86(Generator generator)
        {
            Factor.GenerateX86(generator);

            generator.UnaryOperation(Value);
        }
    }
}