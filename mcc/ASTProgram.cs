using System.Text;

namespace mcc
{
    class ASTProgram : AST
    {
        List<ASTTopLevelItem> TopLevelItemList = new List<ASTTopLevelItem>();

        public override void Parse(Parser parser)
        {
            while (parser.HasMoreTokens())
            {
                ASTTopLevelItem item = new ASTTopLevelItem();
                item.Parse(parser);
                TopLevelItemList.Add(item);
            }
        }

        public override void Print(int indent)
        {
            foreach (var function in TopLevelItemList)
                function.Print(indent);
        }

        public override void GenerateX86(Generator generator)
        {
            foreach (var function in TopLevelItemList)
                function.GenerateX86(generator);

            generator.DefineUninitializedVariables();
        }
    }
}