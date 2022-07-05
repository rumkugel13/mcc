using System.Text;

namespace mcc
{
    class ASTStatement : AST
    {
        public ASTLogicalOrExpression Expression;

        public ASTStatement()
        {
            Expression = new ASTLogicalOrExpression();
        }

        public override void Parse(Parser parser)
        {
            Token token = parser.Next();
            if (token is not Keyword || (token as Keyword).KeywordType != Keyword.KeywordTypes.RETURN)
                parser.Fail(Token.TokenType.KEYWORD, "return");

            Expression.Parse(parser);

            token = parser.Next();
            if (token is not Symbol || (token as Symbol).Value != ';')
                parser.Fail(Token.TokenType.SYMBOL, ";");
        }

        public override void Print(int indent)
        {
            Console.WriteLine(new string(' ', indent) + "RETURN");
            Expression.Print(indent + 3);
        }

        public override void GenerateX86(StringBuilder stringBuilder)
        {
            Expression.GenerateX86(stringBuilder);
            stringBuilder.AppendLine("ret");
        }
    }
}