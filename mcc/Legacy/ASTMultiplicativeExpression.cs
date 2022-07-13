using System.Text;

namespace mcc
{
    class ASTMultiplicativeExpression : ASTAbstractExpression   // = term on website
    {
        public override void Parse(Parser parser)
        {
            Expression = new ASTFactor();
            Expression.Parse(parser);

            while (parser.PeekSymbol('*') || parser.PeekSymbol('/') || parser.PeekSymbol('%'))
            {
                ASTBinaryOperation binaryOperation = new ASTBinaryOperation(new ASTFactor());
                binaryOperation.Parse(parser);
                BinaryOperations.Add(binaryOperation);
            }
        }
    }
}