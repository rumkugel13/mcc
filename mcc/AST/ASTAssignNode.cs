
namespace mcc
{
    class ASTAssignNode : ASTAbstractExpressionNode
    {
        public string Name;
        public ASTAbstractExpressionNode Expression;
        public int Index;
        public bool IsGlobal;
        public bool IsStatement;

        public ASTAssignNode(string id, ASTAbstractExpressionNode expression)
        {
            Name = id;
            Expression = expression;
        }
    }
}