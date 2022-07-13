
namespace mcc
{
    class NodeValidator
    {
        ASTNode rootNode;

        Stack<Dictionary<string, int>> varMaps = new Stack<Dictionary<string, int>>();
        Stack<HashSet<string>> varScopes = new Stack<HashSet<string>>();

        const int varSize = 8; // 32bit = 4, 64bit = 8
        int varOffset = -varSize;

        int loopLabelCounter = 0;
        Stack<int> loops = new Stack<int>();

        public NodeValidator(ASTNode rootNode)
        {
            this.rootNode = rootNode;
        }

        public void Validate(ASTNode node)
        {
            switch (node)
            {
                case ASTNoExpressionNode: break;
                case ASTNoStatementNode: break;
                case ASTProgramNode program: ValidateProgram(program); break;
                case ASTFunctionNode function: ValidateFunction(function); break;
                case ASTReturnNode ret: ValidateReturn(ret); break;
                case ASTConstantNode constant: ValidateConstant(constant); break;
                case ASTUnaryOpNode unOp: ValidateUnaryOp(unOp); break;
                case ASTBinaryOpNode binOp: ValidateBinaryOp(binOp); break;
                case ASTExpressionNode exp: ValidateExpression(exp); break;
                case ASTDeclarationNode dec: ValidateDeclaration(dec); break;
                case ASTAssignNode assign: ValidateAssign(assign); break;
                case ASTVariableNode variable: ValidateVariable(variable); break;
                case ASTConditionNode cond: ValidateCondition(cond); break;
                case ASTConditionalExpressionNode condEx: ValidateConditionalExpression(condEx); break;
                case ASTCompundNode comp: ValidateCompound(comp); break;
                case ASTWhileNode whil: ValidateWhile(whil); break;
                case ASTDoWhileNode doWhil: ValidateDoWhile(doWhil); break;
                case ASTForNode fo: ValidateFor(fo); break;
                case ASTForDeclarationNode forDecl: ValidateForDeclaration(forDecl); break;
                case ASTBreakNode br: ValidateBreak(br); break;
                case ASTContinueNode con: ValidateContinue(con); break;
                default: throw new NotImplementedException("Unkown ASTNode type: " + node.GetType());
            }
        }

        private void ValidateContinue(ASTContinueNode con)
        {
            if (loops.Count == 0)
            {
                throw new ASTLoopScopeException("Fail: Can't continue in non existing loop scope");
            }
            else
            {
                con.LoopCount = loops.Peek();
            }
        }

        private void ValidateBreak(ASTBreakNode br)
        {
            if (loops.Count == 0)
            {
                throw new ASTLoopScopeException("Fail: Can't break out of non existing loop scope");
            }
            else
            {
                br.LoopCount = loops.Peek();
            }
        }

        private void ValidateForDeclaration(ASTForDeclarationNode forDecl)
        {
            forDecl.LoopCount = loopLabelCounter;
            loops.Push(loopLabelCounter++);
            PushScope();
            Validate(forDecl.Declaration);
            Validate(forDecl.Condition);
            PushScope();
            Validate(forDecl.Statement);
            forDecl.BytesToDeallocate = PopScope();
            Validate(forDecl.Post);
            forDecl.BytesToDeallocateInit = PopScope();
        }

        private void ValidateFor(ASTForNode fo)
        {
            fo.LoopCount = loopLabelCounter;
            loops.Push(loopLabelCounter++);
            PushScope();
            Validate(fo.Init);
            Validate(fo.Condition);
            PushScope();
            Validate(fo.Statement);
            fo.BytesToDeallocate = PopScope();
            Validate(fo.Post);
            fo.BytesToDeallocateInit = PopScope();
        }

        private void ValidateDoWhile(ASTDoWhileNode doWhil)
        {
            doWhil.LoopCount = loopLabelCounter;
            loops.Push(loopLabelCounter++);
            PushScope();
            Validate(doWhil.Statement);
            doWhil.BytesToDeallocate = PopScope();
            Validate(doWhil.Expression);
        }

        private void ValidateWhile(ASTWhileNode whil)
        {
            whil.LoopCount = loopLabelCounter;
            loops.Push(loopLabelCounter++);
            Validate(whil.Expression);
            PushScope();
            Validate(whil.Statement);
            whil.BytesToDeallocate = PopScope();
        }

        private void PushScope()
        {
            varMaps.Push(new Dictionary<string, int>(varMaps.Peek()));
            varScopes.Push(new HashSet<string>());
        }

        private int PopScope()
        {
            int newVarCount = varScopes.Peek().Count;
            varMaps.Pop();
            varScopes.Pop();
            varOffset += newVarCount * varSize;
            return newVarCount * varSize;
        }

        private void ValidateCompound(ASTCompundNode comp)
        {
            PushScope();

            foreach (var blockItem in comp.BlockItems)
            {
                Validate(blockItem);
            }

            comp.BytesToDeallocate = PopScope();
        }

        private void ValidateConditionalExpression(ASTConditionalExpressionNode condEx)
        {
            Validate(condEx.Condition);
            Validate(condEx.IfBranch);
            Validate(condEx.ElseBranch);
        }

        private void ValidateCondition(ASTConditionNode cond)
        {
            Validate(cond.Condition);
            Validate(cond.IfBranch);
            if (cond.ElseBranch is not ASTNoStatementNode)
                Validate(cond.ElseBranch);
        }

        private void ValidateVariable(ASTVariableNode variable)
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

        private void ValidateAssign(ASTAssignNode assign)
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

        private void ValidateDeclaration(ASTDeclarationNode dec)
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

        private void ValidateExpression(ASTExpressionNode exp)
        {
            Validate(exp.Expression);
        }

        private void ValidateBinaryOp(ASTBinaryOpNode binOp)
        {
            Validate(binOp.ExpressionLeft);
            Validate(binOp.ExpressionRight);
        }

        private void ValidateUnaryOp(ASTUnaryOpNode unOp)
        {
            Validate(unOp.Expression);
        }

        private void ValidateConstant(ASTConstantNode constant)
        {
            
        }

        private void ValidateReturn(ASTReturnNode ret)
        {
            Validate(ret.Expression);
        }

        private void ValidateFunction(ASTFunctionNode function)
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

        private void ValidateProgram(ASTProgramNode program)
        {
            ValidateFunction(program.Function);
        }
    }
}
