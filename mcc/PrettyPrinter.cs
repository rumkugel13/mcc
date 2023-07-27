using System.Text;

namespace mcc
{
    class PrettyPrinter
    {
        ASTNode rootNode;
        StringBuilder sb = new StringBuilder();
        int indent = 0;

        public PrettyPrinter(ASTNode node)
        {
            rootNode = node;
        }

        public string Print()
        {
            Print(rootNode);
            return sb.ToString();
        }

        private void Print(ASTNode node) 
        { 
            switch (node)
            {
                case ASTNoExpressionNode: break;
                case ASTNoStatementNode: break;
                case ASTProgramNode program: PrintProgram(program); break;
                case ASTFunctionNode function: PrintFunction(function); break;
                case ASTAbstractExpressionNode exp: PrintAbstractExpression(exp); break;
                case ASTBlockItemNode blockItem: PrintBlockItem(blockItem); break;
                default: throw new NotImplementedException("Unkown ASTNode type: " + node.GetType());
            }
        }

        private void PrintProgram(ASTProgramNode program)
        {
            PrintLine("PROGRAM " + program.Name + ":");
            indent++;
            foreach (var topLevelItem in program.TopLevelItems)
                Print(topLevelItem);
            indent--;
        }

        private void PrintFunction(ASTFunctionNode function)
        {
            PrintLine("FUNCTION INT " + function.Name + ":");
            indent++;
            if (function.Parameters.Count > 0)
            {
                PrintLine("PARAMETERS");
                indent++;
                foreach (var parameter in function.Parameters)
                    PrintLine("ID<" + parameter + ">");
                indent--;
            }
            if (function.BlockItems.Count > 0)
            {
                PrintLine("BEGIN_FUNCTION");
                indent++;
                foreach (var blockItem in function.BlockItems)
                    Print(blockItem);
                indent--;
                PrintLine("END_FUNCTION");
            }
            indent--;
        }

        private void PrintBlockItem(ASTBlockItemNode blockItem)
        {
            switch (blockItem)
            {
                case ASTStatementNode statement: PrintStatement(statement); break;
                case ASTDeclarationNode dec: PrintDeclaration(dec); break;
            }
        }

        private void PrintCondition(ASTConditionNode condition)
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

        private void PrintStatement(ASTStatementNode statement)
        {
            switch (statement)
            {
                case ASTReturnNode ret: PrintReturn(ret); break;
                case ASTExpressionNode exp: PrintExpression(exp); break;
                case ASTConditionNode cond: PrintCondition(cond); break;
                case ASTCompundNode comp: PrintCompound(comp); break;
                case ASTWhileNode whil: PrintWhile(whil); break;
                case ASTDoWhileNode doWhil: PrintDoWhile(doWhil); break;
                case ASTForNode fo: PrintFor(fo); break;
                case ASTForDeclarationNode forDecl: PrintDeclaration(forDecl); break;
                case ASTBreakNode br: PrintBreak(br); break;
                case ASTContinueNode con: PrintContinue(con); break;
                default: throw new NotImplementedException("Unkown ASTNode type: " + statement.GetType());
            }
        }

        private void PrintContinue(ASTContinueNode con)
        {
            PrintLine("CONTINUE");
        }

        private void PrintBreak(ASTBreakNode br)
        {
            PrintLine("BREAK");
        }

        private void PrintDeclaration(ASTForDeclarationNode forDecl)
        {
            PrintLine("FOR");
            indent++;
            PrintLine("INIT");
            indent++;
            Print(forDecl.Declaration);
            indent--;
            PrintLine("CONDITION");
            indent++;
            Print(forDecl.Condition);
            indent--;
            PrintLine("POST");
            indent++;
            Print(forDecl.Post);
            indent--;
            PrintLine("BEGINLOOP");
            indent++;
            Print(forDecl.Statement);
            indent--;
            PrintLine("ENDLOOP");
            indent--;
        }

        private void PrintFor(ASTForNode fo)
        {
            PrintLine("FOR");
            indent++;
            PrintLine("INIT");
            indent++;
            Print(fo.Init);
            indent--;
            PrintLine("CONDITION");
            indent++;
            Print(fo.Condition);
            indent--;
            PrintLine("POST");
            indent++;
            Print(fo.Post);
            indent--;
            PrintLine("BEGINLOOP");
            indent++;
            Print(fo.Statement);
            indent--;
            PrintLine("ENDLOOP");
            indent--;
        }

        private void PrintDoWhile(ASTDoWhileNode doWhil)
        {
            PrintLine("DO");
            indent++;
            Print(doWhil.Statement);
            indent--;
            PrintLine("WHILE");
            indent++;
            Print(doWhil.Expression);
            indent--;
        }

        private void PrintWhile(ASTWhileNode whil)
        {
            PrintLine("WHILE");
            indent++;
            Print(whil.Expression);
            PrintLine("DO");
            indent++;
            Print(whil.Statement);
            indent--;
        }

        private void PrintCompound(ASTCompundNode comp)
        {

            PrintLine("BEGINBLOCK");
            indent++;
            foreach (var blockItem in comp.BlockItems)
                Print(blockItem);
            indent--;
            PrintLine("ENDBLOCK");
        }

        private void PrintDeclaration(ASTDeclarationNode dec)
        {
            PrintLine("DECLARE:");
            indent++;
            PrintLine("VAR<" + dec.Name + ">");
            if (dec.Initializer is not ASTNoExpressionNode)
            {
                PrintLine("INITIALIZE:");
                indent++;
                Print(dec.Initializer);
                indent--;
            }
            indent--;
        }

        private void PrintExpression(ASTExpressionNode exp)
        {
            PrintLine("EXPRESSION:");
            indent++;
            Print(exp.Expression);
            indent--;
        }

        private void PrintReturn(ASTReturnNode ret)
        {
            PrintLine("RETURN:");
            indent++;
            Print(ret.Expression);
            indent--;
        }

        private void PrintAbstractExpression(ASTAbstractExpressionNode exp)
        {
            switch (exp)
            {
                case ASTUnaryOpNode unaryOp: PrintUnaryOp(unaryOp); break;
                case ASTConstantNode constant: PrintConstant(constant); break;
                case ASTBinaryOpNode binaryOp: PrintBinaryOp(binaryOp); break;
                case ASTAssignNode assign: PrintAssign(assign); break;
                case ASTVariableNode variable: PrintVariable(variable); break;
                case ASTConditionalExpressionNode cond: PrintConditionalExpression(cond); break;
                case ASTFunctionCallNode funCall: PrintFunctionCall(funCall); break;
                default: throw new NotImplementedException("Unkown ASTNode type: " + exp.GetType());
            }
        }

        private void PrintFunctionCall(ASTFunctionCallNode funCall)
        {
            PrintLine("CALL " + funCall.Name);
            PrintLine("ARGUMENTS");
            indent++;
            foreach (var arg in funCall.Arguments)
                Print(arg);
            indent--;
        }

        private void PrintConditionalExpression(ASTConditionalExpressionNode cond)
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

        private void PrintAssign(ASTAssignNode assign)
        {
            PrintLine("ASSIGN:");
            PrintLine("VAR<" + assign.Name + "> (Stmt: " + assign.IsStatement + ")");
            indent++;
            Print(assign.Expression);
            indent--;
        }

        private void PrintBinaryOp(ASTBinaryOpNode binaryOp)
        {
            Print(binaryOp.ExpressionLeft);
            indent++;
            PrintLine("BinaryOp<" + binaryOp.Value + ">");
            indent--;
            Print(binaryOp.ExpressionRight);
        }

        private void PrintUnaryOp(ASTUnaryOpNode unaryOp)
        {
            PrintLine("UnaryOp<" + unaryOp.Value + ">");
            indent++;
            Print(unaryOp.Expression);
            indent--;
        }

        private void PrintVariable(ASTVariableNode variable)
        {
            PrintLine("VAR<" + variable.Name + ">");
        }

        private void PrintConstant(ASTConstantNode constant)
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