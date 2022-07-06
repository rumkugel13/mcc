using System.Text;

namespace mcc
{
    class ASTConditionalExpression : ASTAbstractExpression
    {
        ASTLogicalOrExpression LogicalOrExpression;
        ASTExpression Expression;
        ASTConditionalExpression ConditionalExpression;

        public override void Parse(Parser parser)
        {
            LogicalOrExpression = new ASTLogicalOrExpression();
            LogicalOrExpression.Parse(parser);

            if (parser.PeekSymbol('?'))
            {
                parser.ExpectSymbol('?');

                Expression = new ASTExpression();
                Expression.Parse(parser);

                parser.ExpectSymbol(':');

                ConditionalExpression = new ASTConditionalExpression();
                ConditionalExpression.Parse(parser);
            }
        }

        public override void Print(int indent)
        {
            if (Expression != null)
            {
                Console.WriteLine(new string(' ', indent) + "CONDITION");
                LogicalOrExpression.Print(indent + 3);
                Console.WriteLine(new string(' ', indent) + "THEN");
                Expression.Print(indent + 3);
                Console.WriteLine(new string(' ', indent) + "ELSE");
                ConditionalExpression.Print(indent + 3);
            }
            else
            {
                LogicalOrExpression.Print(indent);
            }
        }

        public override void GenerateX86(Generator generator)
        {
            if (Expression != null)
            {
                LogicalOrExpression.GenerateX86(generator);
                generator.CompareZero();
                string elseLabel = generator.JumpEqual();
                Expression.GenerateX86(generator);
                string endLabel = generator.Jump();
                generator.Label(elseLabel);
                ConditionalExpression.GenerateX86(generator);
                generator.Label(endLabel);
            }
            else
            {
                LogicalOrExpression.GenerateX86(generator);
            }
        }
    }
}