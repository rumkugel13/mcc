using System.Text;

namespace mcc
{
    class ASTProgram : AST
    {
        public ASTFunction Function;

        public ASTProgram()
        {
            Function = new ASTFunction();
        }

        public override void Parse(Parser parser)
        {
            Function.Parse(parser);

            if (parser.HasMoreTokens())
            {
                parser.Fail("Fail: Too many Tokens");
            }
        }

        public override void Print(int indent)
        {
            Function.Print(0);
        }

        public override void GenerateX86(Generator generator)
        {
            Function.GenerateX86(generator);
        }
    }
}