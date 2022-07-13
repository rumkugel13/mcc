
namespace mcc
{
    class ASTBinaryOpNode : ASTAbstractExpressionNode
    {
        public string Value;
        public ASTAbstractExpressionNode ExpressionLeft, ExpressionRight;

        public ASTBinaryOpNode(string value, ASTAbstractExpressionNode expLeft, ASTAbstractExpressionNode expRight)
        {
            Value = value;
            ExpressionLeft = expLeft;
            ExpressionRight = expRight;
        }
    }
}