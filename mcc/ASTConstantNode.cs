
namespace mcc
{
    class ASTConstantNode : ASTExpressionNode
    {
        public int Value;

        public ASTConstantNode(int value)
        {
            Value = value;
        }
    }
}