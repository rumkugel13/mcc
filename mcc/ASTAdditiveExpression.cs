using System.Text;

namespace mcc
{
    class ASTAdditiveExpression : ASTAbstractExpression
    {
        public override void Parse(Parser parser)
        {
            Expression = new ASTMultiplicativeExpression();
            Expression.Parse(parser);

            Token peek = parser.Peek();
            while ((peek is Symbol && (peek as Symbol).Value == '+') || (peek is Symbol && (peek as Symbol).Value == '-'))
            {
                ASTBinaryOperation binaryOperation = new ASTBinaryOperation(new ASTMultiplicativeExpression());
                binaryOperation.Parse(parser);
                BinaryOperations.Add(binaryOperation);

                peek = parser.Peek();
            }
        }
    }
}