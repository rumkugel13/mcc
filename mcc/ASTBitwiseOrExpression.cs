using System.Text;

namespace mcc
{
    class ASTBitwiseOrExpression : ASTExpression
    {
        public override void Parse(Parser parser)
        {
            Expression = new ASTBitwiseXorExpression();
            Expression.Parse(parser);

            Token peek = parser.Peek();
            while ((peek is Symbol && (peek as Symbol).Value == '|'))
            {
                ASTBinaryOperation binaryOperation = new ASTBinaryOperation(new ASTBitwiseXorExpression());
                binaryOperation.Parse(parser);
                BinaryOperations.Add(binaryOperation);

                peek = parser.Peek();
            }
        }
    }
}