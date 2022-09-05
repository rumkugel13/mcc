
namespace mcc
{
    class ASTReturnNode : ASTStatementNode
    {
        public ASTAbstractExpressionNode Expression;
        public bool IsLastReturn;

        public ASTReturnNode(ASTAbstractExpressionNode expression)
        {
            Expression = expression;
        }
    }
}