
namespace mcc
{
    class ASTWhileNode : ASTStatementNode
    {
        public ASTAbstractExpressionNode Expression;
        public ASTStatementNode Statement;
        public int LoopCount;
        public int VarsToDeallocate;

        public ASTWhileNode(ASTAbstractExpressionNode expression, ASTStatementNode statement)
        {
            Expression = expression;
            Statement = statement;
        }
    }
}