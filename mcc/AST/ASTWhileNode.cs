
namespace mcc
{
    class ASTWhileNode : ASTStatementNode
    {
        public ASTAbstractExpressionNode Expression;
        public ASTStatementNode Statement;
        public int LoopCount;
        public int BytesToDeallocate;

        public ASTWhileNode(ASTAbstractExpressionNode expression, ASTStatementNode statement)
        {
            Expression = expression;
            Statement = statement;
        }
    }
}