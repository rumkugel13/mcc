
namespace mcc
{
    class ASTFunctionCallNode : ASTAbstractExpressionNode
    {
        public string Name;
        public List<ASTAbstractExpressionNode> Arguments;
        public int BytesToDeallocate;

        public ASTFunctionCallNode(string name, List<ASTAbstractExpressionNode> arguments)
        {
            Name = name;
            Arguments = arguments;
        }
    }
}