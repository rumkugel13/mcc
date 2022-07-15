
namespace mcc
{
    class ASTProgramNode : ASTNode
    {
        public string Name;
        public List<ASTTopLevelItemNode> TopLevelItems;
        public List<string> UninitializedGlobalVariables = new List<string>();

        public ASTProgramNode(string programName, List<ASTTopLevelItemNode> topLevelItems)
        {
            Name = programName;
            TopLevelItems = topLevelItems;
        }
    }
}