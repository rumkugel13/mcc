
namespace mcc
{
    class ASTCompundNode : ASTStatementNode
    {
        public List<ASTBlockItemNode> BlockItems;
        public int BytesToPop;

        public ASTCompundNode(List<ASTBlockItemNode> blockItems)
        {
            BlockItems = blockItems;
        }
    }
}