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

        public override void GenerateX86(StringBuilder stringBuilder)
        {
            //// declare variable
            //if (AST.VariableMap.ContainsKey(Identifier.Value))
            //    throw new ASTVariableException("Trying to declare existing Variable: " + Identifier.Value);

            //if (Expression != null)
            //{
            //    Expression.GenerateX86(stringBuilder);
            //}
            //else
            //{
            //    stringBuilder.AppendLine("movq $0, %rax"); // no value given, assign 0
            //}
            //stringBuilder.AppendLine("pushq %rax"); // push current value of variable to stack
            //VariableMap.Add(Identifier.Value, StackIndex);
            //StackIndex -= WordSize;
        }

        public override void GenerateX86(Generator generator)
        {
            //if (Expression != null)
            //{
            //    Expression.GenerateX86(generator);
            //}
            //else
            //{
            //    generator.IntegerConstant(0); // no value given, assign 0
            //}

            //generator.DeclareVariable(Identifier.Value);
        }
    }
}