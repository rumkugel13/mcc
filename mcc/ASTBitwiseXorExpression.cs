using System.Text;

namespace mcc
{
    class ASTBitwiseXorExpression : ASTExpression
    {
        public override void Parse(Parser parser)
        {
            Expression = new ASTBitwiseAndExpression();
            Expression.Parse(parser);

            Token peek = parser.Peek();
            while ((peek is Symbol && (peek as Symbol).Value == '^'))
            {
                ASTBinaryOperation binaryOperation = new ASTBinaryOperation(new ASTBitwiseAndExpression());
                binaryOperation.Parse(parser);
                BinaryOperations.Add(binaryOperation);

                peek = parser.Peek();
            }
        }
    }
}