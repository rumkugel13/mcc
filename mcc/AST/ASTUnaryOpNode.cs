
namespace mcc
{
    class ASTUnaryOpNode : ASTAbstractExpressionNode
    {
        public char Value;
        public ASTAbstractExpressionNode Expression;

        public ASTUnaryOpNode(char value, ASTAbstractExpressionNode expression)
        {
            Value = value;
            Expression = expression;
            this.IsConstantExpression = expression.IsConstantExpression;
        }
    }
}