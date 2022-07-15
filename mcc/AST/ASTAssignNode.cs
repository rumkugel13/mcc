
namespace mcc
{
    class ASTAssignNode : ASTAbstractExpressionNode
    {
        public string Name;
        public ASTAbstractExpressionNode Expression;
        public int Offset;
        public bool IsGlobal;

        public ASTAssignNode(string id, ASTAbstractExpressionNode expression)
        {
            Name = id;
            Expression = expression;
        }
    }
}