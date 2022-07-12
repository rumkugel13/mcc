
namespace mcc
{
    class ASTUnaryOpNode : ASTExpressionNode
    {
        public char Value;
        public ASTExpressionNode Expression;

        public ASTUnaryOpNode(char value, ASTExpressionNode expression)
        {
            Value = value;
            Expression = expression;
        }
    }
}