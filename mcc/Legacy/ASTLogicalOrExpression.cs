using System.Text;

namespace mcc
{
    class ASTLogicalOrExpression : ASTAbstractExpression
    {
        public override void Parse(Parser parser)
        {
            Expression = new ASTLogicalAndExpression();
            Expression.Parse(parser);

            while (parser.PeekSymbol2("||"))
            {
                ASTBinaryOperation binaryOperation = new ASTBinaryOperation(new ASTLogicalAndExpression());
                binaryOperation.Parse(parser);
                BinaryOperations.Add(binaryOperation);
            }
        }
    }
}