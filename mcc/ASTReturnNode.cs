
namespace mcc
{
    class ASTReturnNode : ASTNode
    {
        public ASTConstantNode Constant;

        public ASTReturnNode(ASTConstantNode constant)
        {
            Constant = constant;
        }
    }
}