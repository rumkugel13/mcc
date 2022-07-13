
namespace mcc
{
    class ASTConditionNode : ASTStatementNode
    {
        public ASTAbstractExpressionNode Condition;
        public ASTStatementNode IfBranch, ElseBranch;

        public ASTConditionNode(ASTAbstractExpressionNode condition, ASTStatementNode ifBranch)
        {
            Condition = condition;
            IfBranch = ifBranch;
            ElseBranch = new ASTNoStatementNode();
        }

        public ASTConditionNode(ASTAbstractExpressionNode condition, ASTStatementNode ifBranch, ASTStatementNode elseBranch)
        {
            Condition = condition;
            IfBranch = ifBranch;
            ElseBranch = elseBranch;
        }
    }
}