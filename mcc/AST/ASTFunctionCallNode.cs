
namespace mcc
{
    class ASTFunctionCallNode : ASTAbstractExpressionNode
    {
        public string Name;
        public List<ASTAbstractExpressionNode> Arguments;

        public ASTFunctionCallNode(string name, List<ASTAbstractExpressionNode> arguments)
        {
            Name = name;
            Arguments = arguments;
        }
    }
}