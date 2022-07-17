
namespace mcc
{
    class Validator
    {
        ASTNode rootNode;

        Stack<Dictionary<string, int>> varMaps = new Stack<Dictionary<string, int>>();
        Stack<HashSet<string>> varScopes = new Stack<HashSet<string>>();

        const int pointerSize = 8; // 32bit = 4, 64bit = 8
        const int intSize = 4;
        int varOffset = -pointerSize;   // at rsp+0 is rbp, start at pointer offset

        int loopLabelCounter = 0;
        Stack<int> loops = new Stack<int>();

        struct Function
        {
            public int ParameterCount;
            public bool Defined;
        }

        Dictionary<string, Function> funcMap = new Dictionary<string, Function>();

        const int paramOffset = 2 * pointerSize;    // 1 for return pointer, 2 for old base pointer
        int paramCount = 0;

        Dictionary<string, bool> globalVarMap = new Dictionary<string, bool>();

        public Validator(ASTNode rootNode)
        {
            this.rootNode = rootNode;
        }

        public void ValidateX86()
        {
            Validate(rootNode);
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
                case ASTFunctionCallNode funCall: ValidateFunctionCall(funCall); break;
                default: throw new NotImplementedException("Unkown ASTNode type: " + node.GetType());
            }
        }

        private void ValidateFunctionCall(ASTFunctionCallNode funCall)
        {
            if (funcMap.TryGetValue(funCall.Name, out Function value))
            {
                if (value.ParameterCount != funCall.Arguments.Count)
                    throw new ASTFunctionException("Fail: Trying to call function with too " + 
                        (value.ParameterCount < funCall.Arguments.Count ? "many" : "little") + " parameters: Expected " + 
                         value.ParameterCount + ", Actual " + funCall.Arguments.Count);
            }
            else
                throw new ASTFunctionException("Fail: Trying to call non existing function: " + funCall.Name);

            foreach (var arg in funCall.Arguments)
                Validate(arg);
            funCall.BytesToDeallocate = funCall.Arguments.Count * pointerSize;
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
            varOffset += newVarCount * pointerSize;
            return newVarCount * pointerSize;
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
            else if (globalVarMap.ContainsKey(variable.Name))
            {
                variable.IsGlobal = true;
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
            else if (globalVarMap.ContainsKey(assign.Name))
            {
                Validate(assign.Expression);
                assign.IsGlobal = true;
            }
            else
            {
                throw new ASTVariableException("Fail: Trying to assign to non existing Variable: " + assign.Name);
            }
        }

        private void ValidateDeclaration(ASTDeclarationNode dec)
        {
            if (varScopes.Count == 0)
            {
                ValidateGlobalDeclaration(dec);
                return;
            }

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
            varOffset -= pointerSize;
        }

        private void ValidateGlobalDeclaration(ASTDeclarationNode dec)
        {
            if (globalVarMap.TryGetValue(dec.Name, out bool defined))
            {
                if (defined)
                    throw new ASTVariableException("Fail: Trying to declare existing Global Variable: " + dec.Name);
            }

            if (funcMap.ContainsKey(dec.Name))
            {
                throw new ASTVariableException("Fail: Trying to declare Variable as existing Function: " + dec.Name);
            }

            dec.IsGlobal = true;
            if (dec.Initializer is not ASTNoExpressionNode)
            {
                if (dec.Initializer is not ASTConstantNode)
                {
                    throw new ASTVariableException("Fail: Trying to define non constant to Global Variable: " + dec.Name);
                }
                else
                {
                    Validate(dec.Initializer);
                    dec.GlobalValue = ((ASTConstantNode)dec.Initializer).Value;
                    globalVarMap[dec.Name] = true;
                }
            }
            else
            {
                globalVarMap[dec.Name] = false;
            }
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
            if (!function.IsDefinition)
            {
                // declaration
                if (globalVarMap.ContainsKey(function.Name))
                {
                    throw new ASTFunctionException("Fail: Trying to declare Function as existing Global Variable: " + function.Name);
                }
                else if (funcMap.TryGetValue(function.Name, out Function fun))
                {
                    if (fun.ParameterCount != function.Parameters.Count)
                        throw new ASTFunctionException("Fail: Trying to declare already existing function: " + function.Name);
                }
                else
                {
                    funcMap.Add(function.Name, new Function() { Defined = false, ParameterCount = function.Parameters.Count });
                }
            }
            else
            {
                // definition
                if (globalVarMap.ContainsKey(function.Name))
                {
                    throw new ASTFunctionException("Fail: Trying to define Function as existing Global Variable: " + function.Name);
                }
                else if (funcMap.TryGetValue(function.Name, out Function fun))
                {
                    if (fun.Defined)
                        throw new ASTFunctionException("Fail: Trying to define already existing function: " + function.Name);
                    else if (fun.ParameterCount != function.Parameters.Count)
                        throw new ASTFunctionException("Fail: Trying to define declared function with wrong parameter count: " + function.Name);
                    else funcMap[function.Name] = new Function() { Defined = true, ParameterCount = function.Parameters.Count };
                }
                else
                {
                    funcMap.Add(function.Name, new Function() { Defined = true, ParameterCount = function.Parameters.Count });
                }
            }

            paramCount = 0;
            varOffset = -pointerSize;
            varMaps.Push(new Dictionary<string, int>());
            varScopes.Push(new HashSet<string>());
            bool containsReturn = false;

            foreach (var parameter in function.Parameters)
            {
                varMaps.Peek()[parameter] = paramOffset + paramCount * pointerSize;
                varScopes.Peek().Add(parameter);
                paramCount++;
            }

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
            foreach (var topLevelItem in program.TopLevelItems)
                Validate(topLevelItem);

            foreach (var variable in globalVarMap)
            {
                if (!variable.Value)
                {
                    program.UninitializedGlobalVariables.Add(variable.Key);
                }
            }
        }
    }
}
