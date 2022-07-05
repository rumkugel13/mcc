using System.Text;

namespace mcc
{
    class ASTEqualityExpression : ASTExpression
    {
        ASTRelationalExpression Expression;
        List<ASTBinaryOperation> BinaryOperations = new List<ASTBinaryOperation>();

        public override void Parse(Parser parser)
        {
            Expression = new ASTRelationalExpression();
            Expression.Parse(parser);

            Token peek = parser.Peek();
            while ((peek is Symbol2 && (peek as Symbol2).Value == "!=") || (peek is Symbol2 && (peek as Symbol2).Value == "=="))
            {
                ASTBinaryOperation binaryOperation = new ASTBinaryOperation(new ASTRelationalExpression());
                binaryOperation.Parse(parser);
                BinaryOperations.Add(binaryOperation);

                peek = parser.Peek();
            }
        }

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