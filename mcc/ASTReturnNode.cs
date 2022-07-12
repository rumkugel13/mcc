
namespace mcc
{
    class ASTReturnNode : ASTNode
    {
        public ASTExpressionNode Expression;

        public ASTReturnNode(ASTExpressionNode expression)
        {
            Expression = expression;
        }
    }
}