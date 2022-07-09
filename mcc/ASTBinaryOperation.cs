using System.Text;

namespace mcc
{
    class ASTBinaryOperation : AST
    {
        ASTAbstractExpression Expression;
        string Value;

        public ASTBinaryOperation(ASTAbstractExpression expression)
        {
            Expression = expression;
        }

        public override void Parse(Parser parser)
        {
            Token token = parser.Next();
            if (token is Symbol symbol)
            {
                if (!Symbol.Binary.Contains(symbol.Value))
                {
                    parser.Fail(Token.TokenType.SYMBOL, symbol.Value.ToString());
                }
                else
                {
                    Value = symbol.Value.ToString();
                }
            }
            else if (token is Symbol2 symbol2)
            {
                if (!Symbol2.Dual.Contains(symbol2.Value))
                {
                    parser.Fail(Token.TokenType.SYMBOL2, symbol2.Value);
                }
                else
                {
                    Value = symbol2.Value;
                }
            }
            else
            {
                parser.Fail(Token.TokenType.SYMBOL, "or Symbol2");
            }

            Expression.Parse(parser);
        }

        public override void Print(int indent)
        {
            Console.WriteLine(new String(' ', indent) + "BinOP'" + Value + "'");
            Expression.Print(indent - 3);
        }

        private void GenerateX86ShortCircuit(Generator generator)
        {
            generator.CompareZero();
            string jumpEqualOrNotLabel = "";
            if (Value == "||")
            {
                jumpEqualOrNotLabel = generator.JumpEqual();
                generator.IntegerConstant(1);
            }
            else if (Value == "&&")
            {
                jumpEqualOrNotLabel = generator.JumpNotEqual();
            }

            string endLabel = generator.Jump();
            generator.Label(jumpEqualOrNotLabel);
            Expression.GenerateX86(generator);
            generator.CompareZero();
            generator.IntegerConstant(0);
            generator.Instruction("setne %al");
            generator.Label(endLabel);
        }

        public override void GenerateX86(Generator generator)
        {
            if (Symbol2.ShortCircuit.Contains(Value))
            {
                GenerateX86ShortCircuit(generator);
                return;
            }

            generator.Instruction("pushq %rax");
            Expression.GenerateX86(generator);
            generator.Instruction("movl %eax, %ecx");    // need to switch src and dest for - and /
            generator.Instruction("popq %rax");

            if (Symbol2.Comparison.Contains(Value))
            {
                generator.ComparisonOperation(Value);
            }
            else
            {
                generator.BinaryOperation(Value);
            }
        }
    }
}