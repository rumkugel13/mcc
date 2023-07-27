
namespace mcc
{
    abstract class ASTAbstractExpressionNode : ASTNode
    {
        public bool IsConstantExpression;
        public bool IsStatement;
    }
}