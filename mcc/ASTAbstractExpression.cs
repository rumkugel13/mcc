using System.Text;

namespace mcc
{
    abstract class ASTAbstractExpression : AST
    {
        public ASTAbstractExpression Expression;
        public List<ASTBinaryOperation> BinaryOperations = new List<ASTBinaryOperation>();

        public override void Print(int indent)
        {
            Expression.Print(indent);

            for (int i = 0; i < BinaryOperations.Count; i++)
            {
                BinaryOperations[i].Print(indent + 3);
            }
        }

        public override void GenerateX86(Generator generator)
        {
            Expression.GenerateX86(generator);

            for (int i = 0; i < BinaryOperations.Count; i++)
            {
                BinaryOperations[i].GenerateX86(generator);
            }
        }
    }
}