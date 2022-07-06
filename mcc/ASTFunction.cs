using System.Text;

namespace mcc
{
    class ASTFunction : AST
    {
        public ASTIdentifier Identifier;
        public List<ASTBlockItem> BlockItemList = new List<ASTBlockItem>();
        bool hasReturn;

        public override void Parse(Parser parser)
        {
            Token token = parser.Next();
            if (token is not Keyword || (token as Keyword).KeywordType != Keyword.KeywordTypes.INT)
                parser.Fail(Token.TokenType.KEYWORD, "int");

            Identifier = new ASTIdentifier();
            Identifier.Parse(parser);

            token = parser.Next();
            if (token is not Symbol || (token as Symbol).Value != '(')
                parser.Fail(Token.TokenType.SYMBOL, "(");

            token = parser.Next();
            if (token is not Symbol || (token as Symbol).Value != ')')
                parser.Fail(Token.TokenType.SYMBOL, ")");

            token = parser.Next();
            if (token is not Symbol || (token as Symbol).Value != '{')
                parser.Fail(Token.TokenType.SYMBOL, "{");

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

            token = parser.Next();
            if (token is not Symbol || (token as Symbol).Value != '}')
                parser.Fail(Token.TokenType.SYMBOL, "}");
        }

        public override void Print(int indent)
        {
            Console.WriteLine("FUNC INT " + Identifier.Value + ":");
            Console.WriteLine("   params: ()");
            Console.WriteLine("   body:");

            foreach (var statement in BlockItemList)
                statement.Print(6);
        }

        public override void GenerateX86(Generator generator)
        {
            generator.Instruction(".globl " + Identifier.Value);
            generator.FunctionPrologue(Identifier.Value);

            foreach (var statement in BlockItemList)
                statement.GenerateX86(generator);

            if (!hasReturn)
            {
                // return 0 if no return statement found
                generator.IntegerConstant(0);
                generator.FunctionEpilogue();
            }
        }
    }
}