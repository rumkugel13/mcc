
namespace mcc
{
    class ASTConditionalExpressionNode : ASTAbstractExpressionNode
    {
        public ASTAbstractExpressionNode Condition, IfBranch, ElseBranch;

        public ASTConditionalExpressionNode(ASTAbstractExpressionNode condition, ASTAbstractExpressionNode ifBranch, ASTAbstractExpressionNode elseBranch)
        {
            Condition = condition;
            IfBranch = ifBranch;
            ElseBranch = elseBranch;
        }
    }
}