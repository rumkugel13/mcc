
namespace mcc
{
    class ASTBinaryOpNode : ASTAbstractExpressionNode
    {
        public string Value;
        public ASTAbstractExpressionNode ExpressionLeft, ExpressionRight;
        public bool NeedsShortCircuit, IsComparison;

        public ASTBinaryOpNode(string value, ASTAbstractExpressionNode expLeft, ASTAbstractExpressionNode expRight)
        {
            Value = value;
            ExpressionLeft = expLeft;
            ExpressionRight = expRight;
            this.IsConstantExpression = expLeft.IsConstantExpression && expRight.IsConstantExpression;
        }
    }
}