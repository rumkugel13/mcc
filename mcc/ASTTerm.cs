using System.Text;

namespace mcc
{
    class ASTTerm : AST
    {
        ASTFactor Factor;
        List<ASTBinaryOperation> BinaryOperations = new List<ASTBinaryOperation>();
        List<ASTFactor> SecondFactors = new List<ASTFactor>();

        public override void Parse(Parser parser)
        {
            Factor = new ASTFactor();
            Factor.Parse(parser);

            Token peek = parser.Peek();
            while ((peek is Symbol && (peek as Symbol).Value == '*') || (peek is Symbol && (peek as Symbol).Value == '/'))
            {
                ASTBinaryOperation binaryOperation = new ASTBinaryOperation();
                binaryOperation.Parse(parser);
                BinaryOperations.Add(binaryOperation);

                ASTFactor factor = new ASTFactor();
                factor.Parse(parser);
                SecondFactors.Add(factor);

                peek = parser.Peek();
            }
        }

        public override void Print(int indent)
        {
            if (Factor != null)
            {
                Factor.Print(indent);

                for (int i = 0; i < BinaryOperations.Count; i++)
                {
                    BinaryOperations[i].Print(indent);
                    SecondFactors[i].Print(indent);
                }
            }
        }

        public override void GenerateX86(StringBuilder stringBuilder)
        {
            if (Factor != null)
            {
                Factor.GenerateX86(stringBuilder);

                for (int i = 0; i < BinaryOperations.Count; i++)
                {
                    stringBuilder.AppendLine("pushq %rax");
                    SecondFactors[i].GenerateX86(stringBuilder);
                    stringBuilder.AppendLine("movq %rax, %rcx"); // move factor1 to ecx and push factor2 to acx for - and /
                    stringBuilder.AppendLine("popq %rax");
                    BinaryOperations[i].GenerateX86(stringBuilder);
                }
            }
            else
            {
                throw new InvalidOperationException("Trying to Generate Term with missing Factor");
            }
        }
    }
}