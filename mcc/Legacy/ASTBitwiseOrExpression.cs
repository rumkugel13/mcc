using System.Text;

namespace mcc
{
    class ASTBitwiseOrExpression : ASTAbstractExpression
    {
        public override void Parse(Parser parser)
        {
            Expression = new ASTBitwiseXorExpression();
            Expression.Parse(parser);

            while (parser.PeekSymbol('|'))
            {
                ASTBinaryOperation binaryOperation = new ASTBinaryOperation(new ASTBitwiseXorExpression());
                binaryOperation.Parse(parser);
                BinaryOperations.Add(binaryOperation);
            }
        }
    }
}