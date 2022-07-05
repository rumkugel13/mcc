using System.Text;

namespace mcc
{
    class ASTShiftExpression : ASTAbstractExpression
    {
        public override void Parse(Parser parser)
        {
            Expression = new ASTAdditiveExpression();
            Expression.Parse(parser);

            Token peek = parser.Peek();
            while ((peek is Symbol2 && (peek as Symbol2).Value == "<<") || (peek is Symbol2 && (peek as Symbol2).Value == ">>"))
            {
                ASTBinaryOperation binaryOperation = new ASTBinaryOperation(new ASTAdditiveExpression());
                binaryOperation.Parse(parser);
                BinaryOperations.Add(binaryOperation);

                peek = parser.Peek();
            }
        }
    }
}