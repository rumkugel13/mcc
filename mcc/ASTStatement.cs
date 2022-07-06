using System.Text;

namespace mcc
{
    class ASTStatement : AST
    {
        ASTExpression Expression;
        ASTStatement Statement, OptionalStatement;
        bool isReturn = false;

        public override void Parse(Parser parser)
        {
            if (parser.PeekKeyword(Keyword.KeywordTypes.RETURN))
            {
                isReturn = true;
                parser.ExpectKeyword(Keyword.KeywordTypes.RETURN);

                Expression = new ASTExpression();
                Expression.Parse(parser);

                parser.ExpectSymbol(';');
            }
            else if (parser.PeekKeyword(Keyword.KeywordTypes.IF))
            {
                parser.ExpectKeyword(Keyword.KeywordTypes.IF);
                parser.ExpectSymbol('(');

                Expression = new ASTExpression();
                Expression.Parse(parser);

                parser.ExpectSymbol(')');

                Statement = new ASTStatement();
                Statement.Parse(parser);

                if (parser.PeekKeyword(Keyword.KeywordTypes.ELSE))
                {
                    parser.ExpectKeyword(Keyword.KeywordTypes.ELSE);

                    OptionalStatement = new ASTStatement();
                    OptionalStatement.Parse(parser);
                }
            }
            else
            {
                Expression = new ASTExpression();
                Expression.Parse(parser);

                parser.ExpectSymbol(';');
            }
        }

        public override void Print(int indent)
        {
            if (Statement != null)
            {
                Console.WriteLine(new string(' ', indent) + "IF");

                Expression.Print(indent + 3);

                Console.WriteLine(new string(' ', indent) + "THEN");

                Statement.Print(indent + 3);

                if (OptionalStatement != null)
                {
                    Console.WriteLine(new string(' ', indent) + "ELSE");

                    OptionalStatement.Print(indent + 3);
                }
            }
            else if (isReturn)
            {
                Console.WriteLine(new string(' ', indent) + "RETURN");
                Expression.Print(indent + 3);
            }
            else
            {
                Console.WriteLine(new string(' ', indent) + "EXPR");
                Expression.Print(indent + 3);
            }
        }

        public override void GenerateX86(StringBuilder stringBuilder)
        {
            if (Statement != null)
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
            else if (isReturn)
            {
                Expression.GenerateX86(stringBuilder);
                // func epilogue
                stringBuilder.AppendLine("movq %rbp, %rsp");
                stringBuilder.AppendLine("pop %rbp");

                stringBuilder.AppendLine("ret");
            }
            else
            {
                Expression.GenerateX86(stringBuilder);
            }
        }

        public override void GenerateX86(Generator generator)
        {
            if (Statement != null)
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
            else if (isReturn)
            {
                Expression.GenerateX86(generator);
                generator.FunctionEpilogue();
            }
            else
            {
                Expression.GenerateX86(generator);
            }
        }
    }
}