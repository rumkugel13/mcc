
namespace mcc
{
    class NodeValidator
    {
        ASTNode rootNode;

        Dictionary<string, int> varMap = new Dictionary<string, int>();
        const int varSize = 8; // 32bit = 4, 64bit = 8
        int varOffset = -varSize;

        public NodeValidator(ASTNode rootNode)
        {
            this.rootNode = rootNode;
        }

        public void Validate(ASTNode node)
        {
            switch (node)
            {
                case ASTProgramNode program: ValidateProgramNode(program); break;
                case ASTFunctionNode function: ValidateFunctionNode(function); break;
                case ASTReturnNode ret: ValidateReturnNode(ret); break;
                case ASTConstantNode constant: ValidateConstantNode(constant); break;
                case ASTUnaryOpNode unOp: ValidateUnaryOpNode(unOp); break;
                case ASTBinaryOpNode binOp: ValidateBinaryOpNode(binOp); break;
                case ASTExpressionNode exp: ValidateExpressionNode(exp); break;
                case ASTDeclarationNode dec: ValidateDeclarationNode(dec); break;
                case ASTAssignNode assign: ValidateAssignNode(assign); break;
                case ASTVariableNode variable: ValidateVariableNode(variable); break;
                case ASTConditionNode cond: ValidateConditionNode(cond); break;
                case ASTConditionalExpressionNode condEx: ValidateConditionalExpressionNode(condEx); break;
                default: Console.WriteLine("Fail: Unkown ASTNode type: " + node.GetType()); break;
            }
        }

        private void ValidateConditionalExpressionNode(ASTConditionalExpressionNode condEx)
        {
            Validate(condEx.Condition);
            Validate(condEx.IfBranch);
            Validate(condEx.ElseBranch);
        }

        private void ValidateConditionNode(ASTConditionNode cond)
        {
            Validate(cond.Condition);
            Validate(cond.IfBranch);
            if (cond.ElseBranch is not ASTNoStatementNode)
                Validate(cond.ElseBranch);
        }

        private void ValidateVariableNode(ASTVariableNode variable)
        {
            if (!varMap.ContainsKey(variable.Name))
                throw new ASTVariableException("Fail: Trying to reference a non existing Variable: " + variable);
            else
            {
                variable.Offset = varMap[variable.Name];
            }
        }

        private void ValidateAssignNode(ASTAssignNode assign)
        {
            if (!varMap.ContainsKey(assign.Name))
                throw new ASTVariableException("Fail: Trying to assign to non existing Variable: " + assign.Name);
            else
            {
                Validate(assign.Expression);
                assign.Offset = varMap[assign.Name];
            }
        }

        private void ValidateDeclarationNode(ASTDeclarationNode dec)
        {
            if (varMap.ContainsKey(dec.Name))
                throw new ASTVariableException("Fail: Trying to declare existing Variable: " + dec.Name);
            if (dec.Initializer is not ASTNoExpressionNode)
                Validate(dec.Initializer);

            varMap.Add(dec.Name, varOffset);
            varOffset -= varSize;
        }

        private void ValidateExpressionNode(ASTExpressionNode exp)
        {
            Validate(exp.Expression);
        }

        private void ValidateBinaryOpNode(ASTBinaryOpNode binOp)
        {
            Validate(binOp.ExpressionLeft);
            Validate(binOp.ExpressionRight);
        }

        private void ValidateUnaryOpNode(ASTUnaryOpNode unOp)
        {
            Validate(unOp.Expression);
        }

        private void ValidateConstantNode(ASTConstantNode constant)
        {
            
        }

        private void ValidateReturnNode(ASTReturnNode ret)
        {
            Validate(ret.Expression);
        }

        private void ValidateFunctionNode(ASTFunctionNode function)
        {
            varOffset = -varSize;
            bool containsReturn = false;

            foreach (var statement in function.BlockItems)
            {
                Validate(statement);
                if (statement is ASTReturnNode)
                    containsReturn = true;
            }

            function.ContainsReturn = containsReturn;
        }

        private void ValidateProgramNode(ASTProgramNode program)
        {
            ValidateFunctionNode(program.Function);
        }
    }
}
