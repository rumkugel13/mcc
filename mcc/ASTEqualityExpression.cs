using System.Text;

namespace mcc
{
    class ASTEqualityExpression : ASTAbstractExpression
    {
        public override void Parse(Parser parser)
        {
            Expression = new ASTRelationalExpression();
            Expression.Parse(parser);

            while (parser.PeekSymbol2("!=") || parser.PeekSymbol2("=="))
            {
                ASTBinaryOperation binaryOperation = new ASTBinaryOperation(new ASTRelationalExpression());
                binaryOperation.Parse(parser);
                BinaryOperations.Add(binaryOperation);
            }
        }
    }
}