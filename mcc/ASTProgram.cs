using System.Text;

namespace mcc
{
    class ASTProgram : AST
    {
        List<ASTFunction> FunctionList = new List<ASTFunction>();

        public override void Parse(Parser parser)
        {
            while (parser.HasMoreTokens())
            {
                ASTFunction function = new ASTFunction();
                function.Parse(parser);
                FunctionList.Add(function);
            }
        }

        public override void Print(int indent)
        {
            foreach (var function in FunctionList)
                function.Print(indent);
        }

        public override void GenerateX86(Generator generator)
        {
            foreach (var function in FunctionList)
                function.GenerateX86(generator);
        }
    }
}