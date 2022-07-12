
namespace mcc
{
    class ASTBinaryOpNode : ASTExpressionNode
    {
        public string Value;
        public ASTExpressionNode ExpressionLeft, ExpressionRight;

        public ASTBinaryOpNode(string value, ASTExpressionNode expLeft, ASTExpressionNode expRight)
        {
            Value = value;
            ExpressionLeft = expLeft;
            ExpressionRight = expRight;
        }
    }
}