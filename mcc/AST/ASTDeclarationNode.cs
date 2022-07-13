﻿
namespace mcc
{
    class ASTDeclarationNode : ASTBlockItemNode
    {
        public string Name;
        public ASTAbstractExpressionNode Initializer;

        public ASTDeclarationNode(string id)
        {
            Name = id;
            Initializer = new ASTNoExpressionNode();
        }

        public ASTDeclarationNode(string id, ASTAbstractExpressionNode initializer)
        {
            Name = id;
            Initializer = initializer;
        }
    }
}