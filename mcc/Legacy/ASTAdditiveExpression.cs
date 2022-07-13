using System.Text;

namespace mcc
{
    class ASTAdditiveExpression : ASTAbstractExpression
    {
        public override void Parse(Parser parser)
        {
            Expression = new ASTMultiplicativeExpression();
            Expression.Parse(parser);

            while (parser.PeekSymbol('+') || parser.PeekSymbol('-'))
            {
                ASTBinaryOperation binaryOperation = new ASTBinaryOperation(new ASTMultiplicativeExpression());
                binaryOperation.Parse(parser);
                BinaryOperations.Add(binaryOperation);
            }
        }
    }
}