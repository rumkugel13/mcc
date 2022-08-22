
namespace mcc
{
    class Validator
    {
        ASTNode rootNode;

        Stack<Dictionary<string, int>> varMaps = new Stack<Dictionary<string, int>>();
        Stack<HashSet<string>> varScopes = new Stack<HashSet<string>>();

        const int pointerSize = 8; // 32bit = 4, 64bit = 8
        const int intSize = 4;
        int varOffset = 0;
        int declarationCount = 0;

        int loopLabelCounter = 0;
        Stack<int> loops = new Stack<int>();

        struct Function
        {
            public int ParameterCount;
            public bool Defined;
        }

        Dictionary<string, Function> funcMap = new Dictionary<string, Function>();

        const int paramOffset = 2 * pointerSize;    // 1 for return pointer, 2 for old base pointer

        Dictionary<string, bool> globalVarMap = new Dictionary<string, bool>();

        public Validator(ASTNode rootNode)
        {
            this.rootNode = rootNode;
        }

        public void ValidateX86()
        {
            Validate(rootNode);
        }

        private void Validate(ASTNode node)
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
                {
                    throw new ASTFunctionException($"Fail: Trying to call function {funCall.Name} with too " + 
                        (value.ParameterCount < funCall.Arguments.Count ? "many" : "little") + " parameters: Expected " + 
                         value.ParameterCount + ", Actual " + funCall.Arguments.Count + " at Line: " + funCall.LineNumber + ", Column: " + funCall.LineCharacter);
                }
            }
            else
            {
                FailFunction("Trying to call non existing function", funCall.Name, funCall);
            }

            foreach (var arg in funCall.Arguments)
            {
                Validate(arg);
            }

            // todo: calculate correct bytes based on how many where passed in registers
            funCall.BytesToDeallocate = funCall.Arguments.Count * pointerSize;
        }

        private void ValidateContinue(ASTContinueNode con)
        {
            if (loops.Count == 0)
            {
                FailLoopScope("Can't continue in non existing loop scope", con);
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
                FailLoopScope("Can't break out of non existing loop scope", br);
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
            loops.Pop();
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
            loops.Pop();
        }

        private void ValidateDoWhile(ASTDoWhileNode doWhil)
        {
            doWhil.LoopCount = loopLabelCounter;
            loops.Push(loopLabelCounter++);
            PushScope();
            Validate(doWhil.Statement);
            doWhil.BytesToDeallocate = PopScope();
            Validate(doWhil.Expression);
            loops.Pop();
        }

        private void ValidateWhile(ASTWhileNode whil)
        {
            whil.LoopCount = loopLabelCounter;
            loops.Push(loopLabelCounter++);
            Validate(whil.Expression);
            PushScope();
            Validate(whil.Statement);
            whil.BytesToDeallocate = PopScope();
            loops.Pop();
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
            // update varoffset so that scope variables in stack can be overriden, could leave it for now
            // todo: would need to calculate max simultaneous declared variables first to save on memory
            //varOffset += newVarCount * intSize;
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
            {
                Validate(cond.ElseBranch);
            }
        }

        private void ValidateVariable(ASTVariableNode variable)
        {
            if (varMaps.Count == 0)
            {
                FailVariable("Trying to reference a non Constant Variable", variable.Name, variable);
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
                FailVariable("Trying to reference a non existing Variable", variable.Name, variable);
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
                FailVariable("Trying to assign to non existing Variable", assign.Name, assign);
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
                FailVariable("Trying to declare existing Variable", dec.Name, dec);
            }

            if (dec.Initializer is not ASTNoExpressionNode)
            {
                Validate(dec.Initializer);
            }

            // subtract bytes needed from varOffset
            varOffset -= intSize;
            dec.Offset = varOffset;
            varMaps.Peek()[dec.Name] = varOffset;
            varScopes.Peek().Add(dec.Name);
            declarationCount++;
        }

        private void ValidateGlobalDeclaration(ASTDeclarationNode dec)
        {
            if (globalVarMap.TryGetValue(dec.Name, out bool defined))
            {
                if (defined)
                {
                    FailVariable("Trying to declare existing Global Variable", dec.Name, dec);
                }
            }

            if (funcMap.ContainsKey(dec.Name))
            {
                FailVariable("Trying to declare Variable as existing Function", dec.Name, dec);
            }

            dec.IsGlobal = true;
            if (dec.Initializer is not ASTNoExpressionNode)
            {
                if (dec.Initializer is not ASTConstantNode)
                {
                    FailVariable("Trying to assign non constant value to Global Variable", dec.Name, dec);
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
                    FailFunction("Trying to declare Function as existing Global Variable", function.Name, function);
                }
                else if (funcMap.TryGetValue(function.Name, out Function fun))
                {
                    if (fun.ParameterCount != function.Parameters.Count)
                    {
                        FailFunction("Trying to declare already existing function", function.Name, function);
                    }
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
                    FailFunction("Trying to define Function as existing Global Variable", function.Name, function);
                }
                else if (funcMap.TryGetValue(function.Name, out Function fun))
                {
                    if (fun.Defined)
                    {
                        FailFunction("Trying to define already defined function", function.Name, function);
                    }
                    else if (fun.ParameterCount != function.Parameters.Count)
                    {
                        FailFunction("Trying to define declared function with wrong parameter count", function.Name, function);
                    }
                    else
                    {
                        funcMap[function.Name] = new Function() { Defined = true, ParameterCount = function.Parameters.Count };
                    }
                }
                else
                {
                    funcMap.Add(function.Name, new Function() { Defined = true, ParameterCount = function.Parameters.Count });
                }
            }

            varOffset = 0;
            declarationCount = 0;
            varMaps.Push(new Dictionary<string, int>());
            varScopes.Push(new HashSet<string>());

            for (int i = 0; i < function.Parameters.Count; i++)
            {
                int offset;
                //if (System.Runtime.InteropServices.RuntimeInformation.OSArchitecture == System.Runtime.InteropServices.Architecture.Arm64)
                {
                    varOffset -= intSize; 
                    declarationCount++; // count function parameters as declaration
                    // todo: use paramoffset for parameters on stack
                    offset = varOffset;
                }
                //else
                //{
                //    offset = paramOffset + i * pointerSize;
                //}
                
                string? parameter = function.Parameters[i];
                varMaps.Peek()[parameter] = offset;
                varScopes.Peek().Add(parameter);
            }

            bool containsReturn = false;
            foreach (var blockItem in function.BlockItems)
            {
                Validate(blockItem);
                if (blockItem is ASTReturnNode)
                {
                    containsReturn = true;
                }
            }

            function.ContainsReturn = containsReturn;

            // 16 byte aligned
            // todo: calculate max simultaneous declared variables to save on memory
            function.BytesToAllocate = 16 * ((declarationCount * intSize + 15) / 16);

            varMaps.Pop();
            varScopes.Pop();
        }

        private void ValidateProgram(ASTProgramNode program)
        {
            foreach (var topLevelItem in program.TopLevelItems)
            {
                Validate(topLevelItem);
            }

            foreach (var variable in globalVarMap)
            {
                if (!variable.Value)
                {
                    program.UninitializedGlobalVariables.Add(variable.Key);
                }
                program.GlobalVariables.Add(variable.Key);
            }
        }

        private void FailVariable(string msg, string name, ASTNode node)
        {
            throw new ASTVariableException($"Fail: {msg}: {name} at Line: {node.LineNumber}, Column: {node.LineCharacter}");
        }

        private void FailFunction(string msg, string name, ASTNode node)
        {
            throw new ASTFunctionException($"Fail: {msg}: {name} at Line: {node.LineNumber}, Column: {node.LineCharacter}");
        }

        private void FailLoopScope(string msg, ASTNode node)
        {
            throw new ASTLoopScopeException($"Fail: {msg} at Line: {node.LineNumber}, Column: {node.LineCharacter}");
        }
    }
}
