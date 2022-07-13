
namespace mcc
{
    class ASTReturnNode : ASTStatementNode
    {
        public ASTAbstractExpressionNode Expression;

        public ASTReturnNode(ASTAbstractExpressionNode expression)
        {
            Expression = expression;
        }
    }
}