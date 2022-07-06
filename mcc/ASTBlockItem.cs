using System.Text;

namespace mcc
{
    class ASTBlockItem : AST
    {
        ASTDeclaration Declaration;
        ASTStatement Statement;

        public override void Parse(Parser parser)
        {
            if (parser.PeekKeyword(Keyword.KeywordTypes.INT))
            {
                Declaration = new ASTDeclaration();
                Declaration.Parse(parser);
            }
            else
            {
                Statement = new ASTStatement();
                Statement.Parse(parser);
            }
        }

        public override void Print(int indent)
        {
            if (Declaration != null)
            {
                Declaration.Print(indent);
            }
            else
            {
                Statement.Print(indent);
            }
        }

        public override void GenerateX86(Generator generator)
        {
            if (Declaration != null)
            {
                Declaration.GenerateX86(generator);
            }
            else
            {
                Statement.GenerateX86(generator);
            }
        }
    }
}