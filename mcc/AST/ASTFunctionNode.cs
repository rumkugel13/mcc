
namespace mcc
{
    class ASTFunctionNode : ASTTopLevelItemNode
    {
        public string Name;
        public List<string> Parameters;
        public List<ASTBlockItemNode> BlockItems;
        public bool IsDefinition;
        public bool ContainsReturn;

        public ASTFunctionNode(string name, List<string> parameters, List<ASTBlockItemNode> blockItems)
        {
            Name = name;
            Parameters = parameters;
            BlockItems = blockItems;
            IsDefinition = true;
        }

        public ASTFunctionNode(string name, List<string> parameters)
        {
            Name = name;
            Parameters = parameters;
            BlockItems = new List<ASTBlockItemNode>();
        }
    }
}