using System.Text;

namespace mcc
{
    class ASTShiftExpression : ASTAbstractExpression
    {
        public override void Parse(Parser parser)
        {
            Expression = new ASTAdditiveExpression();
            Expression.Parse(parser);

            while (parser.PeekSymbol2("<<") || parser.PeekSymbol2(">>"))
            {
                ASTBinaryOperation binaryOperation = new ASTBinaryOperation(new ASTAdditiveExpression());
                binaryOperation.Parse(parser);
                BinaryOperations.Add(binaryOperation);
            }
        }
    }
}