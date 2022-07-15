
namespace mcc
{
    class ASTDeclarationNode : ASTTopLevelItemNode
    {
        public string Name;
        public ASTAbstractExpressionNode Initializer;
        public bool IsGlobal;
        public int GlobalValue;

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