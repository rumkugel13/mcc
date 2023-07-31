
namespace mcc
{
    internal class Evaluator
    {
        public static int Evaluate(ASTAbstractExpressionNode exp)
        {
            switch (exp)
            {
                case ASTConstantNode constant: return EvaluateConstant(constant);
                case ASTUnaryOpNode unary: return EvaluateUnaryOp(unary);
                case ASTBinaryOpNode binary: return EvaluateBinaryOp(binary);
                default: return 0;
            }
        }

        public static int EvaluateConstant(ASTConstantNode constant)
        {
            return constant.Value;
        }

        public static int EvaluateUnaryOp(ASTUnaryOpNode unaryOp)
        {
            switch (unaryOp.Value)
            {
                case '-': return -Evaluate(unaryOp.Expression);
                case '~': return ~Evaluate(unaryOp.Expression);
                case '!': return Evaluate(unaryOp.Expression) == 0 ? 1 : 0;
                case '+':
                default: return Evaluate(unaryOp.Expression);
            }
        }

        public static int EvaluateBinaryOp(ASTBinaryOpNode binOp)
        {
            if (binOp.Value == "||")
            {
                if (Evaluate(binOp.ExpressionLeft) != 0)
                {
                    return 1;
                }
                else
                {
                    return Evaluate(binOp.ExpressionRight) != 0 ? 1 : 0;
                }
            }
            else if (binOp.Value == "&&")
            {
                if (Evaluate(binOp.ExpressionLeft) == 0)
                {
                    return 0;
                }
                else
                {
                    return Evaluate(binOp.ExpressionRight) != 0 ? 1 : 0;
                }
            }
            else
            {
                switch (binOp.Value)
                {
                    case "+": return Evaluate(binOp.ExpressionLeft) + Evaluate(binOp.ExpressionRight);
                    case "*": return Evaluate(binOp.ExpressionLeft) * Evaluate(binOp.ExpressionRight);
                    case "-": return Evaluate(binOp.ExpressionLeft) - Evaluate(binOp.ExpressionRight);
                    case "<<": return Evaluate(binOp.ExpressionLeft) << Evaluate(binOp.ExpressionRight);
                    case ">>": return Evaluate(binOp.ExpressionLeft) >> Evaluate(binOp.ExpressionRight);
                    case "&": return Evaluate(binOp.ExpressionLeft) & Evaluate(binOp.ExpressionRight);
                    case "|": return Evaluate(binOp.ExpressionLeft) | Evaluate(binOp.ExpressionRight);
                    case "^": return Evaluate(binOp.ExpressionLeft) ^ Evaluate(binOp.ExpressionRight);
                    case "/": return Evaluate(binOp.ExpressionLeft) / Evaluate(binOp.ExpressionRight);
                    case "%": return Evaluate(binOp.ExpressionLeft) % Evaluate(binOp.ExpressionRight);
                    case "==": return Evaluate(binOp.ExpressionLeft) == Evaluate(binOp.ExpressionRight) ? 1 : 0;
                    case "!=": return Evaluate(binOp.ExpressionLeft) != Evaluate(binOp.ExpressionRight) ? 1 : 0;
                    case ">=": return Evaluate(binOp.ExpressionLeft) >= Evaluate(binOp.ExpressionRight) ? 1 : 0;
                    case ">": return Evaluate(binOp.ExpressionLeft) > Evaluate(binOp.ExpressionRight) ? 1 : 0;
                    case "<=": return Evaluate(binOp.ExpressionLeft) <= Evaluate(binOp.ExpressionRight) ? 1 : 0;
                    case "<": return Evaluate(binOp.ExpressionLeft) < Evaluate(binOp.ExpressionRight) ? 1 : 0;
                    default: return 0;
                }
            }
        }
    }
}
