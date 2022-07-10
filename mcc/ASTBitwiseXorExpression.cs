using System.Text;

namespace mcc
{
    class ASTBitwiseXorExpression : ASTAbstractExpression
    {
        public override void Parse(Parser parser)
        {
            Expression = new ASTBitwiseAndExpression();
            Expression.Parse(parser);

            while (parser.PeekSymbol('^'))
            {
                ASTBinaryOperation binaryOperation = new ASTBinaryOperation(new ASTBitwiseAndExpression());
                binaryOperation.Parse(parser);
                BinaryOperations.Add(binaryOperation);
            }
        }
    }
}