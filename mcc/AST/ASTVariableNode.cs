
namespace mcc
{
    class ASTVariableNode : ASTAbstractExpressionNode
    {
        public string Name;
        public int Offset;
        public bool IsGlobal;

        public ASTVariableNode(string id)
        {
            Name = id;
        }
    }
}