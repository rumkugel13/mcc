
namespace mcc
{
    class ASTProgramNode : ASTNode
    {
        public string Name;
        public ASTFunctionNode Function;

        public ASTProgramNode(string programName, ASTFunctionNode function)
        {
            Name = programName;
            Function = function;
        }
    }
}