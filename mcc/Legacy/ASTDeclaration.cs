using System.Text;

namespace mcc
{
    class ASTDeclaration : AST
    {
        ASTExpression Expression;
        ASTIdentifier Identifier;

        public override void Parse(Parser parser)
        {
            // declaration
            parser.ExpectKeyword(Keyword.KeywordTypes.INT);

            Identifier = new ASTIdentifier();
            Identifier.Parse(parser);

            if (parser.PeekSymbol('='))
            {
                // assignment
                parser.ExpectSymbol('=');

                Expression = new ASTExpression();
                Expression.Parse(parser);
            }

            parser.ExpectSymbol(';');
        }

        public override void Print(int indent)
        {
            Console.WriteLine(new string(' ', indent) + "DECLARE");
            Identifier.Print(indent + 3);

            if (Expression != null)
            {
                Console.WriteLine(new string(' ', indent + 3) + "ASSIGN");
                Expression.Print(indent + 6);
            }
        }

        public override void GenerateX86(Generator generator)
        {
            if (generator.IsGlobalVariable())
            {
                generator.DeclareGlobalVariable(Identifier.Value);

                if (Expression != null)
                    Expression.GenerateX86(generator);
                return;
            }

            if (Expression != null)
            {
                Expression.GenerateX86(generator);
            }
            else
            {
                generator.IntegerConstant(0); // no value given, assign 0
            }

            generator.DeclareVariable(Identifier.Value);
        }
    }
}