using System.Text;

namespace mcc
{
    class ASTLogicalOrExpression : ASTExpression    // = exp on website
    {
        public override void Parse(Parser parser)
        {
            Expression = new ASTLogicalAndExpression();
            Expression.Parse(parser);

            Token peek = parser.Peek();
            while ((peek is Symbol2 && (peek as Symbol2).Value == "||"))
            {
                ASTBinaryOperation binaryOperation = new ASTBinaryOperation(new ASTLogicalAndExpression());
                binaryOperation.Parse(parser);
                BinaryOperations.Add(binaryOperation);

                peek = parser.Peek();
            }
        }
    }
}