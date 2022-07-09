using System.Text;

namespace mcc
{
    class ASTFunction : AST
    {
        ASTIdentifier Identifier;
        List<ASTIdentifier> Parameters = new List<ASTIdentifier>();
        List<ASTBlockItem> BlockItemList = new List<ASTBlockItem>();
        bool hasReturn;
        bool isDeclaration;

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
                isDeclaration = true;
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
            Console.WriteLine("   PARAMS");

            foreach (var p in Parameters)
            {
                Console.WriteLine(new string(' ', 6) + "INT ID<" + p.Value + ">");
            }

            Console.WriteLine("   BODY");

            Console.WriteLine(new string(' ', 6) + "BLK_BEGIN");
            foreach (var statement in BlockItemList)
                statement.Print(9);
            Console.WriteLine(new string(' ', 6) + "BLK_END");
        }

        public override void GenerateX86(Generator generator)
        {
            if (isDeclaration)
            {
                // declaration
                generator.DeclareFunction(Identifier.Value, Parameters.Count);
                return;
            }

            // definition
            generator.Instruction(".globl " + Identifier.Value);
            generator.FunctionPrologue(Identifier.Value, Parameters.Count);

            generator.BeginBlock();

            foreach (var param in Parameters)
                generator.DeclareParameter(param.Value);

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