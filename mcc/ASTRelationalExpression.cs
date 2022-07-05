using System.Text;

namespace mcc
{
    class ASTRelationalExpression : ASTAbstractExpression
    {
        public override void Parse(Parser parser)
        {
            Expression = new ASTShiftExpression();
            Expression.Parse(parser);

            Token peek = parser.Peek();
            while ((peek is Symbol && (peek as Symbol).Value == '<') || (peek is Symbol && (peek as Symbol).Value == '>') ||
                    (peek is Symbol2 && (peek as Symbol2).Value == "<=") || (peek is Symbol2 && (peek as Symbol2).Value == ">="))
            {
                ASTBinaryOperation binaryOperation = new ASTBinaryOperation(new ASTShiftExpression());
                binaryOperation.Parse(parser);
                BinaryOperations.Add(binaryOperation);

                peek = parser.Peek();
            }
        }
    }
}