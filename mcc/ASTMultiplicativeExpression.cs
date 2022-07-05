using System.Text;

namespace mcc
{
    class ASTMultiplicativeExpression : ASTExpression   // = term on website
    {
        public override void Parse(Parser parser)
        {
            Expression = new ASTFactor();
            Expression.Parse(parser);

            Token peek = parser.Peek();
            while ((peek is Symbol && (peek as Symbol).Value == '*') || (peek is Symbol && (peek as Symbol).Value == '/') || (peek is Symbol && (peek as Symbol).Value == '%'))
            {
                ASTBinaryOperation binaryOperation = new ASTBinaryOperation(new ASTFactor());
                binaryOperation.Parse(parser);
                BinaryOperations.Add(binaryOperation);

                peek = parser.Peek();
            }
        }
    }
}