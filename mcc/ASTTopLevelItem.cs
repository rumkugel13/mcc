using System.Text;

namespace mcc
{
    class ASTTopLevelItem : AST
    {
        ASTDeclaration Declaration;
        ASTFunction Function;

        public override void Parse(Parser parser)
        {
            if (parser.PeekKeyword(Keyword.KeywordTypes.INT))
            {
                if (parser.Peek(2) is Symbol symbol && symbol.Value == '(')
                {
                    Function = new ASTFunction();
                    Function.Parse(parser);
                }
                else
                {
                    Declaration = new ASTDeclaration();
                    Declaration.Parse(parser);
                }
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
                Function.Print(indent);
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
                Function.GenerateX86(generator);
            }
        }
    }
}