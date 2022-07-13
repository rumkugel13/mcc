
namespace mcc
{
    class ASTConstantNode : ASTAbstractExpressionNode
    {
        public int Value;

        public ASTConstantNode(int value)
        {
            Value = value;
        }
    }
}