using System.Text;

namespace mcc
{
    class ASTExpression : AST
    {
        ASTTerm Term;
        List<ASTBinaryOperation> BinaryOperations = new List<ASTBinaryOperation>();
        List<ASTTerm> SecondTerms = new List<ASTTerm>();

        public ASTExpression()
        {

        }

        public override void Parse(Parser parser)
        {
            Term = new ASTTerm();
            Term.Parse(parser);

            Token peek = parser.Peek();
            while ((peek is Symbol && (peek as Symbol).Value == '+') || (peek is Symbol && (peek as Symbol).Value == '-'))
            {
                ASTBinaryOperation binaryOperation = new ASTBinaryOperation();
                binaryOperation.Parse(parser);
                BinaryOperations.Add(binaryOperation);

                ASTTerm term = new ASTTerm();
                term.Parse(parser);
                SecondTerms.Add(term);

                peek = parser.Peek();
            }
        }

        public override void Print(int indent)
        {
            Term.Print(indent);

            for (int i = 0; i < BinaryOperations.Count; i++)
            {
                BinaryOperations[i].Print(indent);
                SecondTerms[i].Print(indent);
            }
        }

        public override void GenerateX86(StringBuilder stringBuilder)
        {
            if (Term != null)
            {
                Term.GenerateX86(stringBuilder);

                for (int i = 0; i < BinaryOperations.Count; i++)
                {
                    stringBuilder.AppendLine("pushq %rax");
                    SecondTerms[i].GenerateX86(stringBuilder);
                    stringBuilder.AppendLine("popq %rcx");
                    BinaryOperations[i].GenerateX86(stringBuilder);
                }
            }
            else
            {
                throw new InvalidOperationException("Trying to Generate Expression with missing Term");
            }
        }
    }
}