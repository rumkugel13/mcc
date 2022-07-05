using System.Text;

namespace mcc
{
    class ASTFunction : AST
    {
        public ASTIdentifier Identifier;
        public List<ASTStatement> StatementList = new List<ASTStatement>();

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
                ASTStatement statement = new ASTStatement();
                statement.Parse(parser);
                StatementList.Add(statement);
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

            foreach (var statement in StatementList)
                statement.Print(6);
        }

        public override void GenerateX86(StringBuilder stringBuilder)
        {
            stringBuilder.AppendLine(".globl " + Identifier.Value);
            stringBuilder.AppendLine("" + Identifier.Value + ":");

            foreach (var statement in StatementList)
                statement.GenerateX86(stringBuilder);
        }
    }
}