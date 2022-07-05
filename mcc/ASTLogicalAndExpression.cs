using System.Text;

namespace mcc
{
    class ASTLogicalAndExpression : ASTAbstractExpression
    {
        public override void Parse(Parser parser)
        {
            Expression = new ASTBitwiseOrExpression();
            Expression.Parse(parser);

            Token peek = parser.Peek();
            while ((peek is Symbol2 && (peek as Symbol2).Value == "&&"))
            {
                ASTBinaryOperation binaryOperation = new ASTBinaryOperation(new ASTBitwiseOrExpression());
                binaryOperation.Parse(parser);
                BinaryOperations.Add(binaryOperation);

                peek = parser.Peek();
            }
        }
    }
}