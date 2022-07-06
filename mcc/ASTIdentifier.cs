using System.Text;

namespace mcc
{
    class ASTIdentifier : AST
    {
        public string Value;

        public ASTIdentifier()
        {
            Value = "";
        }

        public override void Parse(Parser parser)
        {
            Token token = parser.Next();
            if (token is not Identifier)
                parser.Fail(Token.TokenType.IDENTIFIER);

            Value = (token as Identifier).Value;
        }

        public override void Print(int indent)
        {
            Console.WriteLine(new String(' ', indent) + "ID<" + Value + ">");
        }
    }
}