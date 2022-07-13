
namespace mcc
{
    class ASTFunctionNode : ASTNode
    {
        public string Name;
        public List<ASTBlockItemNode> BlockItems;
        public bool ContainsReturn;

        public ASTFunctionNode(string name, List<ASTBlockItemNode> blockItems)
        {
            Name = name;
            BlockItems = blockItems;
        }
    }
}