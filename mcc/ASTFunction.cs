using System.Text;

namespace mcc
{
    class ASTFunction : AST
    {
        public ASTIdentifier Identifier;
        public ASTStatement Statement;

        public ASTFunction()
        {
            Identifier = new ASTIdentifier();
            Statement = new ASTStatement();
        }

        public override void Parse(Parser parser)
        {
            Token token = parser.Next();
            if (token is not Keyword || (token as Keyword).KeywordType != Keyword.KeywordTypes.INT)
                parser.Fail(Token.TokenType.KEYWORD, "int");

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

            Statement.Parse(parser);

            token = parser.Next();
            if (token is not Symbol || (token as Symbol).Value != '}')
                parser.Fail(Token.TokenType.SYMBOL, "}");
        }

        public override void Print(int indent)
        {
            Console.WriteLine("FUNC INT " + Identifier.Value + ":");
            Console.WriteLine("   params: ()");
            Console.WriteLine("   body:");
            Statement.Print(6);
        }

        public override void GenerateX86(StringBuilder stringBuilder)
        {
            stringBuilder.AppendLine(".globl " + Identifier.Value);
            stringBuilder.AppendLine("" + Identifier.Value + ":");
            Statement.GenerateX86(stringBuilder);
        }
    }
}