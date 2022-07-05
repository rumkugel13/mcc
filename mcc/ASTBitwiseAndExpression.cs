using System.Text;

namespace mcc
{
    class ASTBitwiseAndExpression : ASTExpression
    {
        public override void Parse(Parser parser)
        {
            Expression = new ASTEqualityExpression();
            Expression.Parse(parser);

            Token peek = parser.Peek();
            while ((peek is Symbol && (peek as Symbol).Value == '&'))
            {
                ASTBinaryOperation binaryOperation = new ASTBinaryOperation(new ASTEqualityExpression());
                binaryOperation.Parse(parser);
                BinaryOperations.Add(binaryOperation);

                peek = parser.Peek();
            }
        }
    }
}