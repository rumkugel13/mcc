
namespace mcc
{
    class ASTExpressionNode : ASTStatementNode
    {
        public ASTAbstractExpressionNode Expression;

        public ASTExpressionNode(ASTAbstractExpressionNode expression)
        {
            Expression = expression;
        }
    }
}