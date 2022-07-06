using System.Text;

namespace mcc
{
    class ASTDeclaration : AST
    {
        ASTExpression Expression;
        ASTIdentifier Identifier;

        public override void Parse(Parser parser)
        {
            // declaration
            parser.ExpectKeyword(Keyword.KeywordTypes.INT);

            Identifier = new ASTIdentifier();
            Identifier.Parse(parser);

            if (parser.PeekSymbol('='))
            {
                // assignment
                parser.ExpectSymbol('=');

                Expression = new ASTExpression();
                Expression.Parse(parser);
            }

            parser.ExpectSymbol(';');
        }

        public override void Print(int indent)
        {
            Console.WriteLine(new string(' ', indent) + "DECLARE");
            Identifier.Print(indent + 3);

            if (Expression != null)
            {
                Console.WriteLine(new string(' ', indent + 3) + "ASSIGN");
                Expression.Print(indent + 6);
            }
        }

        public override void GenerateX86(StringBuilder stringBuilder)
        {
            // declare variable
            if (AST.VariableMap.ContainsKey(Identifier.Value))
                throw new ASTVariableException("Trying to declare existing Variable: " + Identifier.Value);

            if (Expression != null)
            {
                Expression.GenerateX86(stringBuilder);
            }
            else
            {
                stringBuilder.AppendLine("movq $0, %rax"); // no value given, assign 0
            }
            stringBuilder.AppendLine("pushq %rax"); // push current value of variable to stack
            VariableMap.Add(Identifier.Value, StackIndex);
            StackIndex -= WordSize;
        }

        public override void GenerateX86(Generator generator)
        {
            if (Expression != null)
            {
                Expression.GenerateX86(generator);
            }
            else
            {
                generator.IntegerConstant(0); // no value given, assign 0
            }

            generator.DeclareVariable(Identifier.Value);
        }
    }
}