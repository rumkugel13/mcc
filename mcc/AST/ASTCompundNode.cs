
namespace mcc
{
    class ASTCompundNode : ASTStatementNode
    {
        public List<ASTBlockItemNode> BlockItems;
        public int BytesToDeallocate;

        public ASTCompundNode(List<ASTBlockItemNode> blockItems)
        {
            BlockItems = blockItems;
        }
    }
}