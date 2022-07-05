using System.Text;

namespace mcc
{
    class ASTMultiplicativeExpression : ASTExpression   // = term on website
    {
        //ASTFactor Factor;
        //List<ASTBinaryOperation> BinaryOperations = new List<ASTBinaryOperation>();

        public override void Parse(Parser parser)
        {
            Expression = new ASTFactor();
            Expression.Parse(parser);

            Token peek = parser.Peek();
            while ((peek is Symbol && (peek as Symbol).Value == '*') || (peek is Symbol && (peek as Symbol).Value == '/') || (peek is Symbol && (peek as Symbol).Value == '%'))
            {
                ASTBinaryOperation binaryOperation = new ASTBinaryOperation(new ASTFactor());
                binaryOperation.Parse(parser);
                BinaryOperations.Add(binaryOperation);

                peek = parser.Peek();
            }
        }

        //public override void Print(int indent)
        //{
        //    Factor.Print(indent);

        //    for (int i = 0; i < BinaryOperations.Count; i++)
        //    {
        //        BinaryOperations[i].Print(indent + 3);
        //    }
        //}

        //public override void GenerateX86(StringBuilder stringBuilder)
        //{
        //    Factor.GenerateX86(stringBuilder);

        //    for (int i = 0; i < BinaryOperations.Count; i++)
        //    {
        //        BinaryOperations[i].GenerateX86(stringBuilder);
        //    }
        //}
    }
}