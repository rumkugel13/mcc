
namespace mcc
{
    class NodeValidator
    {
        ASTNode rootNode;

        Stack<Dictionary<string, int>> varMaps = new Stack<Dictionary<string, int>>();
        Stack<HashSet<string>> varScopes = new Stack<HashSet<string>>();

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
                case ASTCompundNode comp: ValidateCompoundNode(comp); break;
                default: Console.WriteLine("Fail: Unkown ASTNode type: " + node.GetType()); break;
            }
        }

        private void ValidateCompoundNode(ASTCompundNode comp)
        {
            varMaps.Push(new Dictionary<string, int>(varMaps.Peek()));
            varScopes.Push(new HashSet<string>());

            foreach (var blockItem in comp.BlockItems)
            {
                Validate(blockItem);
            }

            int newVarCount = varScopes.Peek().Count;
            varMaps.Pop();
            varScopes.Pop();
            varOffset += newVarCount * varSize;
            comp.BytesToPop = newVarCount * varSize;
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
            if (varMaps.Count == 0)
            {
                throw new ASTVariableException("Fail: Trying to reference a non Constant Variable: " + variable.Name);
            }
            else if (varMaps.Peek().TryGetValue(variable.Name, out int offset))
            {
                variable.Offset = offset;
            }
            else
            {
                throw new ASTVariableException("Fail: Trying to reference a non existing Variable: " + variable.Name);
            }
        }

        private void ValidateAssignNode(ASTAssignNode assign)
        {
            if (varMaps.Peek().TryGetValue(assign.Name, out int offset))
            {
                Validate(assign.Expression);
                assign.Offset = offset;
            }
            else
            {
                throw new ASTVariableException("Fail: Trying to assign to non existing Variable: " + assign.Name);
            }
        }

        private void ValidateDeclarationNode(ASTDeclarationNode dec)
        {
            if (varScopes.Peek().Contains(dec.Name))
            {
                throw new ASTVariableException("Fail: Trying to declare existing Variable: " + dec.Name);
            }

            if (dec.Initializer is not ASTNoExpressionNode)
            {
                Validate(dec.Initializer);
            }

            varMaps.Peek()[dec.Name] = varOffset;
            varScopes.Peek().Add(dec.Name);
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
            varMaps.Push(new Dictionary<string, int>());
            varScopes.Push(new HashSet<string>());
            bool containsReturn = false;

            foreach (var blockItem in function.BlockItems)
            {
                Validate(blockItem);
                if (blockItem is ASTReturnNode)
                    containsReturn = true;
            }

            function.ContainsReturn = containsReturn;

            varMaps.Pop();
            varScopes.Pop();
        }

        private void ValidateProgramNode(ASTProgramNode program)
        {
            ValidateFunctionNode(program.Function);
        }
    }
}
