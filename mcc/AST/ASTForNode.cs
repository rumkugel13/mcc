
namespace mcc
{
    class ASTForNode : ASTStatementNode
    {
        public ASTStatementNode Statement;
        public ASTAbstractExpressionNode Condition, Init, Post;
        public int LoopCount;
        public int BytesToDeallocateInit, BytesToDeallocate;

        public ASTForNode(ASTStatementNode body, ASTAbstractExpressionNode condition)
        {
            Statement = body;
            Condition = condition;
            Init = new ASTNoExpressionNode();
            Post = new ASTNoExpressionNode();
        }

        public ASTForNode(ASTStatementNode body, ASTAbstractExpressionNode init, ASTAbstractExpressionNode condition) : this(body, condition)
        {
            Init = init;
        }

        public ASTForNode(ASTAbstractExpressionNode condition, ASTAbstractExpressionNode post, ASTStatementNode body) : this(body, condition)
        {
            Post = post;
        }

        public ASTForNode(ASTStatementNode body, ASTAbstractExpressionNode init, ASTAbstractExpressionNode condition, ASTAbstractExpressionNode post) : this(body, condition)
        {
            Init = init;
            Post = post;
        }
    }
}