
namespace mcc
{
    class ASTFunctionNode : ASTTopLevelItemNode
    {
        public string Name;

        public struct Parameter
        {
            public string Name;
            public int Offset;
        }

        public List<Parameter> Parameters;
        public List<ASTBlockItemNode> BlockItems;
        public bool IsDefinition;
        public bool ContainsReturn;
        public int BytesToAllocate; // allocate bytes to stack for alignment, TODO: move var decls using rbp instead of push

        public ASTFunctionNode(string name, List<Parameter> parameters, List<ASTBlockItemNode> blockItems)
        {
            Name = name;
            Parameters = parameters;
            BlockItems = blockItems;
            IsDefinition = true;
        }

        public ASTFunctionNode(string name, List<Parameter> parameters)
        {
            Name = name;
            Parameters = parameters;
            BlockItems = new List<ASTBlockItemNode>();
        }
    }
}