﻿
namespace mcc
{
    class ASTCompundNode : ASTStatementNode
    {
        public List<ASTBlockItemNode> BlockItems;
        public int VarsToDeallocate;

        public ASTCompundNode(List<ASTBlockItemNode> blockItems)
        {
            BlockItems = blockItems;
        }
    }
}