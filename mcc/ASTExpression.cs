using System.Text;

namespace mcc
{
    abstract class ASTExpression : AST
    {
        public ASTExpression Expression;
        public List<ASTBinaryOperation> BinaryOperations = new List<ASTBinaryOperation>();

        public override void Print(int indent)
        {
            Expression.Print(indent);

            for (int i = 0; i < BinaryOperations.Count; i++)
            {
                BinaryOperations[i].Print(indent + 3);
            }
        }

        public override void GenerateX86(StringBuilder stringBuilder)
        {
            Expression.GenerateX86(stringBuilder);

            for (int i = 0; i < BinaryOperations.Count; i++)
            {
                BinaryOperations[i].GenerateX86(stringBuilder);
            }
        }
    }
}