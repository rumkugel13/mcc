using System.Text;

namespace mcc
{
    class ASTBinaryOperation : AST
    {
        public static int LabelCounter = 0;
        public ASTAbstractExpression Expression;
        public string Value;

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

        private void GenerateX86ShortCircuit(StringBuilder stringBuilder)
        {
            int labelCounter = LabelCounter++;
            stringBuilder.AppendLine("cmpq $0, %rax");
            if (Value == "||")
            {
                stringBuilder.AppendLine("je _label" + labelCounter);
                stringBuilder.AppendLine("movq $1, %rax");
            }
            else if (Value == "&&")
            {
                stringBuilder.AppendLine("jne _label" + labelCounter);
            }
            stringBuilder.AppendLine("jmp _end" + labelCounter);
            stringBuilder.AppendLine("_label" + labelCounter + ":");
            Expression.GenerateX86(stringBuilder);
            stringBuilder.AppendLine("cmpq $0, %rax");
            stringBuilder.AppendLine("movq $0, %rax");
            stringBuilder.AppendLine("setne %al");
            stringBuilder.AppendLine("_end" + labelCounter + ":");
        }

        public override void GenerateX86(StringBuilder stringBuilder)
        {
            if (Symbol2.ShortCircuit.Contains(Value))
            {
                GenerateX86ShortCircuit(stringBuilder);
                return;
            }

            stringBuilder.AppendLine("pushq %rax");
            Expression.GenerateX86(stringBuilder);
            stringBuilder.AppendLine("movq %rax, %rcx");    // need to switch src and dest for - and /
            stringBuilder.AppendLine("popq %rax");

            if (Symbol2.Comparison.Contains(Value))
            {
                stringBuilder.AppendLine("cmpq %rcx, %rax");
                stringBuilder.AppendLine("movq $0, %rax");
            }

            switch (Value)
            {
                case "+": stringBuilder.AppendLine("addq %rcx, %rax"); break;
                case "*": stringBuilder.AppendLine("imulq %rcx, %rax"); break;
                case "-": stringBuilder.AppendLine("subq %rcx, %rax"); break;
                case "/":
                    stringBuilder.AppendLine("cdq");
                    stringBuilder.AppendLine("idivl %ecx");
                    break;
                case "%":
                    stringBuilder.AppendLine("cdq");
                    stringBuilder.AppendLine("idivl %ecx");
                    stringBuilder.AppendLine("movq %rdx, %rax");
                    break;
                case "==": stringBuilder.AppendLine("sete %al"); break;
                case "!=": stringBuilder.AppendLine("setne %al"); break;
                case ">=": stringBuilder.AppendLine("setge %al"); break;
                case ">": stringBuilder.AppendLine("setg %al"); break;
                case "<=": stringBuilder.AppendLine("setle %al"); break;
                case "<": stringBuilder.AppendLine("setl %al"); break;
                case "<<": stringBuilder.AppendLine("sal %rcx, %rax"); break;
                case ">>": stringBuilder.AppendLine("sar %rcx, %rax"); break;
                case "&": stringBuilder.AppendLine("and %rcx, %rax"); break;
                case "|": stringBuilder.AppendLine("or %rcx, %rax"); break;
                case "^": stringBuilder.AppendLine("xor %rcx, %rax"); break;
                default:
                    break;
            }
        }
    }
}