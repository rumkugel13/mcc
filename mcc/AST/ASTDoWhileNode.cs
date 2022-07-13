
namespace mcc
{
    class ASTDoWhileNode : ASTStatementNode
    {
        public ASTStatementNode Statement;
        public ASTAbstractExpressionNode Expression;
        public int LoopCount;
        public int BytesToDeallocate;

        public ASTDoWhileNode(ASTStatementNode statement, ASTAbstractExpressionNode expression)
        {
            Statement = statement;
            Expression = expression;
        }
    }
}