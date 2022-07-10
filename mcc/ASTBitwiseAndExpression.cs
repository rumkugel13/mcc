using System.Text;

namespace mcc
{
    class ASTBitwiseAndExpression : ASTAbstractExpression
    {
        public override void Parse(Parser parser)
        {
            Expression = new ASTEqualityExpression();
            Expression.Parse(parser);

            while (parser.PeekSymbol('&'))
            {
                ASTBinaryOperation binaryOperation = new ASTBinaryOperation(new ASTEqualityExpression());
                binaryOperation.Parse(parser);
                BinaryOperations.Add(binaryOperation);
            }
        }
    }
}