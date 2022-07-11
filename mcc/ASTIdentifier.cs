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
            parser.ExpectIdentifier(out string value);

            Value = value;
        }

        public override void Print(int indent)
        {
            Console.WriteLine(new String(' ', indent) + "ID<" + Value + ">");
        }
    }
}