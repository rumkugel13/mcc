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
        }

        public override void Print(int indent)
        {
            Function.Print(0);
        }

        public override void GenerateX86(StringBuilder stringBuilder)
        {
            Function.GenerateX86(stringBuilder);
        }
    }
}