using System.Text;

namespace mcc
{
    class ASTFunction : AST
    {
        public ASTIdentifier Identifier;
        public List<ASTIdentifier> Parameters = new List<ASTIdentifier>();
        public List<ASTBlockItem> BlockItemList = new List<ASTBlockItem>();
        bool hasReturn;

        public override void Parse(Parser parser)
        {
            parser.ExpectKeyword(Keyword.KeywordTypes.INT);

            Identifier = new ASTIdentifier();
            Identifier.Parse(parser);

            parser.ExpectSymbol('(');

            if (parser.PeekKeyword(Keyword.KeywordTypes.INT))
            {
                parser.ExpectKeyword(Keyword.KeywordTypes.INT);
                ASTIdentifier id = new ASTIdentifier();
                id.Parse(parser);
                Parameters.Add(id);

                while (parser.PeekSymbol(','))
                {
                    parser.ExpectSymbol(',');

                    parser.ExpectKeyword(Keyword.KeywordTypes.INT);
                    id = new ASTIdentifier();
                    id.Parse(parser);
                    Parameters.Add(id);
                }
            }

            parser.ExpectSymbol(')');

            if (parser.PeekSymbol(';'))
            {
                parser.ExpectSymbol(';');
            }
            else
            {
                parser.ExpectSymbol('{');

                while (!parser.PeekSymbol('}'))
                {
                    if (parser.PeekKeyword(Keyword.KeywordTypes.RETURN))
                    {
                        hasReturn = true;
                    }

                    ASTBlockItem blockItem = new ASTBlockItem();
                    blockItem.Parse(parser);
                    BlockItemList.Add(blockItem);
                }

                parser.ExpectSymbol('}');
            }
        }

        public override void Print(int indent)
        {
            Console.WriteLine("FUNC INT " + Identifier.Value + ":");
            Console.WriteLine("   params: ()");
            Console.WriteLine("   body:");

            Console.WriteLine(new string(' ', 6) + "BLK_BEGIN");
            foreach (var statement in BlockItemList)
                statement.Print(9);
            Console.WriteLine(new string(' ', 6) + "BLK_END");
        }

        public override void GenerateX86(Generator generator)
        {
            generator.Instruction(".globl " + Identifier.Value);
            generator.FunctionPrologue(Identifier.Value);

            generator.BeginBlock();

            foreach (var statement in BlockItemList)
                statement.GenerateX86(generator);

            generator.EndBlock();

            if (!hasReturn)
            {
                // return 0 if no return statement found
                generator.IntegerConstant(0);
                generator.FunctionEpilogue();
            }
        }
    }
}