
namespace mcc
{
    class ASTConstantNode : ASTNode
    {
        public int Value;

        public ASTConstantNode(int value)
        {
            Value = value;
        }
    }
}