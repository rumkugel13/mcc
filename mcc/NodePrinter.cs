using System.Text;

namespace mcc
{
    class NodePrinter
    {
        StringBuilder sb = new StringBuilder();
        int indent = 0;

        public override string ToString()
        {
            return sb.ToString();
        }

        public void Print(ASTNode node) 
        { 
            switch (node)
            {
                case ASTProgramNode program:
                    PrintProgramNode(program);
                    break;
                case ASTFunctionNode function:
                    PrintFunctionNode(function);
                    break;
                case ASTReturnNode ret:
                    PrintReturnNode(ret);
                    break;
                case ASTExpressionNode exp:
                    PrintExpressionNode(exp);
                    break;
            }
        }

        private void PrintProgramNode(ASTProgramNode program)
        {
            PrintLine("PROGRAM " + program.Name + ":");
            indent++;
            Print(program.Function);
            indent--;
        }

        private void PrintFunctionNode(ASTFunctionNode function)
        {
            PrintLine("FUNCTION INT " + function.Name + ":");
            indent++;
            Print(function.Return);
            indent--;
        }

        private void PrintReturnNode(ASTReturnNode ret)
        {
            PrintLine("RETURN:");
            indent++;
            Print(ret.Expression);
            indent--;
        }

        private void PrintExpressionNode(ASTExpressionNode exp)
        {
            switch (exp)
            {
                case ASTUnaryOpNode unaryOp: PrintUnaryOpNode(unaryOp); break;
                case ASTConstantNode constant: PrintConstantNode(constant); break;
                case ASTBinaryOpNode binaryOp: PrintBinaryOpNode(binaryOp); break;
            }
        }

        private void PrintBinaryOpNode(ASTBinaryOpNode binaryOp)
        {
            indent++;
            Print(binaryOp.ExpressionLeft);
            indent--;
            PrintLine("BinaryOp<" + binaryOp.Value + ">");
            indent++;
            Print(binaryOp.ExpressionRight);
            indent--;
        }

        private void PrintUnaryOpNode(ASTUnaryOpNode unaryOp)
        {
            PrintLine("UnaryOp<" + unaryOp.Value + ">");
            indent++;
            Print(unaryOp.Expression);
            indent--;
        }

        private void PrintConstantNode(ASTConstantNode constant)
        {
            PrintLine("INT<" + constant.Value + ">");
        }

        private void PrintLine(string line)
        {
            sb.AppendLine(Indent() + line);
        }

        private string Indent()
        {
            return new string(' ', indent * 4);
        }
    }
}