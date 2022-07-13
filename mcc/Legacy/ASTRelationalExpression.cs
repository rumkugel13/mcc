using System.Text;

namespace mcc
{
    class ASTRelationalExpression : ASTAbstractExpression
    {
        public override void Parse(Parser parser)
        {
            Expression = new ASTShiftExpression();
            Expression.Parse(parser);

            while (parser.PeekSymbol('<') || parser.PeekSymbol('>') ||
                   parser.PeekSymbol2("<=") || parser.PeekSymbol2(">="))
            {
                ASTBinaryOperation binaryOperation = new ASTBinaryOperation(new ASTShiftExpression());
                binaryOperation.Parse(parser);
                BinaryOperations.Add(binaryOperation);
            }
        }
    }
}