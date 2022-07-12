
namespace mcc
{
    class ASTBinaryOpNode : ASTExpressionNode
    {
        public char Value;
        public ASTExpressionNode ExpressionLeft, ExpressionRight;

        public ASTBinaryOpNode(char value, ASTExpressionNode expLeft, ASTExpressionNode expRight)
        {
            Value = value;
            ExpressionLeft = expLeft;
            ExpressionRight = expRight;
        }
    }
}