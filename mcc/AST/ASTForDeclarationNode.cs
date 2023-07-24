
namespace mcc
{
    class ASTForDeclarationNode : ASTStatementNode
    {
        public ASTStatementNode Statement;
        public ASTAbstractExpressionNode Condition, Post;
        public ASTDeclarationNode Declaration;
        public int LoopCount;
        public int VarsToDeallocate;
        public int VarsToDeallocateInit;

        public ASTForDeclarationNode(ASTStatementNode body, ASTDeclarationNode declaration, ASTAbstractExpressionNode condition)
        {
            Statement = body;
            Condition = condition;
            Declaration = declaration;
            Post = new ASTNoExpressionNode();
        }

        public ASTForDeclarationNode(ASTStatementNode body, ASTDeclarationNode declaration, ASTAbstractExpressionNode condition, ASTAbstractExpressionNode post) : this(body, declaration, condition)
        {
            Post = post;
        }
    }
}