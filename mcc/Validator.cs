namespace mcc
{
    class Validator
    {
        ASTNode rootNode;

        readonly Stack<Dictionary<string, int>> varMaps = new Stack<Dictionary<string, int>>();

        int totalVarDeclCount = 0;
        int returnCount = 0;

        int loopLabelCounter = 0;
        readonly Stack<int> loops = new Stack<int>();

        struct Function
        {
            public int ParameterCount;
            public bool Defined;
        }

        readonly Dictionary<string, Function> funcMap = new Dictionary<string, Function>();
        readonly Dictionary<string, bool> globalVarMap = new Dictionary<string, bool>();

        public Validator(ASTNode rootNode)
        {
            this.rootNode = rootNode;
        }

        public void ValidateAST()
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
            forDecl.VarsToDeallocate = PopScope();
            Validate(forDecl.Post);
            forDecl.VarsToDeallocateInit = PopScope();
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
            fo.VarsToDeallocate = PopScope();
            Validate(fo.Post);
            fo.VarsToDeallocateInit = PopScope();
            loops.Pop();
        }

        private void ValidateDoWhile(ASTDoWhileNode doWhil)
        {
            doWhil.LoopCount = loopLabelCounter;
            loops.Push(loopLabelCounter++);
            PushScope();
            Validate(doWhil.Statement);
            doWhil.VarsToDeallocate = PopScope();
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
            whil.VarsToDeallocate = PopScope();
            loops.Pop();
        }

        private void PushScope()
        {
            varMaps.Push(new Dictionary<string, int>(varMaps.Peek()));
        }

        private int PopScope()
        {
            int newVarCount = varMaps.Pop().Count;
            // update varoffset so that scope variables in stack can be overridden, could leave it for now
            // todo: would need to calculate max simultaneous declared variables first to save on memory
            return newVarCount;
        }

        private void ValidateCompound(ASTCompundNode comp)
        {
            PushScope();

            foreach (var blockItem in comp.BlockItems)
            {
                Validate(blockItem);
            }

            comp.VarsToDeallocate = PopScope();
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
            else if (varMaps.Peek().TryGetValue(variable.Name, out int index))
            {
                variable.Index = index;
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
            if (varMaps.Peek().TryGetValue(assign.Name, out int index))
            {
                Validate(assign.Expression);
                assign.Index = index;
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
            if (varMaps.Count == 0)
            {
                ValidateGlobalDeclaration(dec);
                return;
            }

            if (varMaps.Peek().ContainsKey(dec.Name))
            {
                FailVariable("Trying to declare existing Variable", dec.Name, dec);
            }

            if (dec.Initializer is not ASTNoExpressionNode)
            {
                Validate(dec.Initializer);
            }

            dec.Index = totalVarDeclCount;
            varMaps.Peek()[dec.Name] = totalVarDeclCount;
            totalVarDeclCount++;
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
                if (!dec.Initializer.IsConstantExpression)
                {
                    FailVariable("Trying to assign non constant value/expression to Global Variable", dec.Name, dec);
                }
                else
                {
                    Validate(dec.Initializer);
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
            returnCount++;
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
                else if (new HashSet<string>(function.Parameters).Count != function.Parameters.Count)
                {
                    FailFunction("Duplicate argument names for function", function.Name, function);
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
                    else if (new HashSet<string>(function.Parameters).Count != function.Parameters.Count)
                    {
                        FailFunction("Duplicate argument names for function", function.Name, function);
                    }
                    else
                    {
                        funcMap[function.Name] = new Function() { Defined = true, ParameterCount = function.Parameters.Count };
                    }
                }
                else if (new HashSet<string>(function.Parameters).Count != function.Parameters.Count)
                {
                    FailFunction("Duplicate argument names for function", function.Name, function);
                }
                else
                {
                    funcMap.Add(function.Name, new Function() { Defined = true, ParameterCount = function.Parameters.Count });
                }
            }

            totalVarDeclCount = 0;
            returnCount = 0;
            varMaps.Push(new Dictionary<string, int>());

            foreach (string? parameter in function.Parameters)
            {
                varMaps.Peek()[parameter] = totalVarDeclCount;
                totalVarDeclCount++;
            }

            bool containsReturn = false;
            foreach (var blockItem in function.BlockItems)
            {
                Validate(blockItem);
                if (blockItem is ASTReturnNode ret)
                {
                    if (containsReturn)
                    {
                        Fail("Duplicate return statement", ret);
                        return;
                    }

                    containsReturn = true;
                    ret.IsLastReturn = true;
                    // todo: everything after this is unreachable code
                }
            }

            function.ContainsReturn = containsReturn;
            function.ReturnCount = returnCount;
            function.TotalVarDeclCount = totalVarDeclCount;

            varMaps.Pop();
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

        private void Fail(string msg, ASTNode node)
        {
            throw new ASTFunctionException($"Fail: {msg} at Line: {node.LineNumber}, Column: {node.LineCharacter}");
        }
    }
}
