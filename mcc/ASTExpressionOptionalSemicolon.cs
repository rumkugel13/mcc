using System.Text;

namespace mcc
{
    class ASTExpressionOptionalSemicolon : ASTAbstractExpression
    {
        ASTAbstractExpression Expression;

        public override void Parse(Parser parser)
        {
            if (!parser.PeekSymbol(';'))
            {
                Expression = new ASTExpression();
                Expression.Parse(parser);
            }

            parser.ExpectSymbol(';');
        }

        public override void Print(int indent)
        {
            if (Expression != null)
            {
                Expression.Print(indent);
            }
        }

        public override void GenerateX86(Generator generator)
        {
            if (Expression != null)
            {
                Expression.GenerateX86(generator);
            }
        }
    }
}