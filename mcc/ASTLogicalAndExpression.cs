using System.Text;

namespace mcc
{
    class ASTLogicalAndExpression : ASTAbstractExpression
    {
        public override void Parse(Parser parser)
        {
            Expression = new ASTBitwiseOrExpression();
            Expression.Parse(parser);

            while (parser.PeekSymbol2("&&"))
            {
                ASTBinaryOperation binaryOperation = new ASTBinaryOperation(new ASTBitwiseOrExpression());
                binaryOperation.Parse(parser);
                BinaryOperations.Add(binaryOperation);
            }
        }
    }
}