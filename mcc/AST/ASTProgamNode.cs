
namespace mcc
{
    class ASTProgramNode : ASTNode
    {
        public string Name;
        public List<ASTFunctionNode> Functions;

        public ASTProgramNode(string programName, List<ASTFunctionNode> functions)
        {
            Name = programName;
            Functions = functions;
        }
    }
}