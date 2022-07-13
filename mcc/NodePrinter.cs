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
                case ASTProgramNode program: PrintProgramNode(program); break;
                case ASTFunctionNode function: PrintFunctionNode(function); break;
                case ASTAbstractExpressionNode exp: PrintAbstractExpressionNode(exp); break;
                case ASTStatementNode statement: PrintStatementNode(statement); break;
                default: PrintLine("Unkown ASTNode type: " + node.GetType()); break;
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
            foreach (var statement in function.BlockItems)
                Print(statement);
            indent--;
        }

        private void PrintBlockItemNode(ASTBlockItemNode blockItem)
        {
            switch (blockItem)
            {
                case ASTStatementNode statement: PrintStatementNode(statement); break;
                case ASTDeclarationNode dec: PrintDeclarationNode(dec); break;
            }
        }

        private void PrintConditionNode(ASTConditionNode condition)
        {
            PrintLine("IF");
            indent++;
            Print(condition.Condition);
            indent--;
            PrintLine("THEN");
            indent++;
            Print(condition.IfBranch);
            indent--;

            if (condition.ElseBranch is not ASTNoStatementNode)
            {
                PrintLine("ELSE");
                indent++;
                Print(condition.ElseBranch);
                indent--;
            }
        }

        private void PrintStatementNode(ASTStatementNode statement)
        {
            switch (statement)
            {
                case ASTReturnNode ret: PrintReturnNode(ret); break;
                case ASTExpressionNode exp: PrintExpressionNode(exp); break;
                case ASTConditionNode cond: PrintConditionNode(cond); break;
                default: PrintLine("Unkown ASTNode type: " + statement.GetType()); break;
            }
        }

        private void PrintDeclarationNode(ASTDeclarationNode dec)
        {
            PrintLine("DECLARE:");
            indent++;
            PrintLine("VAR<" + dec.Name + ">");
            if (dec.Initializer is not ASTNoExpressionNode)
            {
                PrintLine("ASSIGN:");
                indent++;
                Print(dec.Initializer);
                indent--;
            }
            indent--;
        }

        private void PrintExpressionNode(ASTExpressionNode exp)
        {
            PrintLine("EXPRESSION:");
            indent++;
            Print(exp.Expression);
            indent--;
        }

        private void PrintReturnNode(ASTReturnNode ret)
        {
            PrintLine("RETURN:");
            indent++;
            Print(ret.Expression);
            indent--;
        }

        private void PrintAbstractExpressionNode(ASTAbstractExpressionNode exp)
        {
            switch (exp)
            {
                case ASTUnaryOpNode unaryOp: PrintUnaryOpNode(unaryOp); break;
                case ASTConstantNode constant: PrintConstantNode(constant); break;
                case ASTBinaryOpNode binaryOp: PrintBinaryOpNode(binaryOp); break;
                case ASTAssignNode assign: PrintAssignNode(assign); break;
                case ASTVariableNode variable: PrintVariableNode(variable); break;
                case ASTConditionalExpressionNode cond: PrintConditionalExpressionNode(cond); break;
                default: PrintLine("Unkown ASTNode type: " + exp.GetType()); break;
            }
        }

        private void PrintConditionalExpressionNode(ASTConditionalExpressionNode cond)
        {
            PrintLine("CONDITION");
            indent++;
            Print(cond.Condition);
            indent--;
            PrintLine("THEN");
            indent++;
            Print(cond.IfBranch);
            indent--;
            PrintLine("ELSE");
            indent++;
            Print(cond.ElseBranch);
            indent--;
        }

        private void PrintAssignNode(ASTAssignNode assign)
        {
            PrintLine("ASSIGN:");
            PrintLine("VAR<" + assign.Name + ">");
            indent++;
            Print(assign.Expression);
            indent--;
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

        private void PrintVariableNode(ASTVariableNode variable)
        {
            PrintLine("VAR<" + variable.Name + ">");
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