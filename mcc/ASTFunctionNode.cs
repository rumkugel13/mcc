
namespace mcc
{
    class ASTFunctionNode : ASTNode
    {
        public string Name;
        public ASTReturnNode Return;

        public ASTFunctionNode(string name, ASTReturnNode returnNode)
        {
            Name = name;
            Return = returnNode;
        }
    }
}