namespace mcc
{
    class Interpreter
    {
        ASTProgramNode rootNode;
        Dictionary<string, Function> functions = new();
        Dictionary<string, int> globalVariables = new();
        Stack<Dictionary<string, int>> variables = new();
        Stack<string> functionsStack = new();
        bool hasReturned, isBreak, isContinue;

        struct Function
        {
            public ASTFunctionNode FunctionNode;
            public List<Parameter> Parameters;
            public int ReturnValue;
        }

        struct Parameter
        {
            public string Name;
            public int Value;
        }

        public Interpreter(ASTProgramNode rootNode)
        {
            this.rootNode = rootNode;
        }

        public int Interpret()
        {
            // define functions and global vars
            InterpretProgram(rootNode);

            // call main, assuming main exists without parameters
            InterpretFunction(functions["main"].FunctionNode);

            return functions["main"].ReturnValue;
        }

        private int Interpret(ASTNode node)
        {
            if (hasReturned || isContinue || isBreak)
                return 0;

            switch (node)
            {
                case ASTNoExpressionNode: break;
                case ASTNoStatementNode: break;
                case ASTProgramNode program: InterpretProgram(program); break;
                case ASTFunctionNode function: return InterpretFunction(function);
                case ASTReturnNode ret: InterpretReturn(ret); break;
                case ASTConstantNode constant: return InterpretConstant(constant);
                case ASTUnaryOpNode unOp: return InterpretUnaryOp(unOp);
                case ASTBinaryOpNode binOp: return InterpretBinaryOp(binOp);
                case ASTExpressionNode exp: return InterpretExpression(exp);
                case ASTDeclarationNode dec: InterpretDeclaration(dec); break;
                case ASTAssignNode assign: return InterpretAssign(assign);
                case ASTVariableNode variable: return InterpretVariable(variable);
                case ASTConditionNode cond: InterpretCondition(cond); break;
                case ASTConditionalExpressionNode condEx: return InterpretConditionalExpression(condEx);
                case ASTCompundNode comp: InterpretCompound(comp); break;
                case ASTWhileNode whil: InterpretWhile(whil); break;
                case ASTDoWhileNode doWhil: InterpretDoWhile(doWhil); break;
                case ASTForNode fo: InterpretFor(fo); break;
                case ASTForDeclarationNode forDecl: InterpretForDeclaration(forDecl); break;
                case ASTBreakNode br: InterpretBreak(br); break;
                case ASTContinueNode con: InterpretContinue(con); break;
                case ASTFunctionCallNode funCall: return InterpretFunctionCall(funCall);
                default: throw new NotImplementedException("Unkown ASTNode type: " + node.GetType());
            }

            return 0;
        }

        private int InterpretFunctionCall(ASTFunctionCallNode funCall)
        {
            // note: this is a std function which is defined externally
            if (funCall.Name == "putchar")
            {
                Console.Write(Convert.ToChar(Interpret(funCall.Arguments[0])));
                return 0;
            }

            for (int i = funCall.Arguments.Count - 1; i >= 0; i--)
            {
                Parameter parameter = functions[funCall.Name].Parameters[i];
                parameter.Value = Interpret(funCall.Arguments[i]);
                functions[funCall.Name].Parameters[i] = parameter;
            }

            InterpretFunction(functions[funCall.Name].FunctionNode);

            return functions[funCall.Name].ReturnValue;
        }

        private void InterpretContinue(ASTContinueNode con)
        {
            isContinue = true;
        }

        private void InterpretBreak(ASTBreakNode br)
        {
            isBreak = true;
        }

        private void InterpretForDeclaration(ASTForDeclarationNode forDecl)
        {
            PushScope();
            for (Interpret(forDecl.Declaration); Interpret(forDecl.Condition) != 0; Interpret(forDecl.Post))
            {
                PushScope();
                Interpret(forDecl.Statement);
                PopScope();
                if (isBreak)
                {
                    isBreak = false;
                    break;
                }
                isContinue = false;
            }
            PopScope();
        }

        private void InterpretFor(ASTForNode fo)
        {
            PushScope();
            for (Interpret(fo.Init); Interpret(fo.Condition) != 0; Interpret(fo.Post))
            {
                PushScope();
                Interpret(fo.Statement);
                PopScope();
                if (isBreak)
                {
                    isBreak = false;
                    break;
                }
                isContinue = false;
            }
            PopScope();
        }

        private void InterpretDoWhile(ASTDoWhileNode doWhil)
        {
            do
            {
                PushScope();
                Interpret(doWhil.Statement);
                PopScope();
                if (isBreak)
                {
                    isBreak = false;
                    break;
                }
                isContinue = false;
            }
            while (Interpret(doWhil.Expression) != 0);
        }

        private void InterpretWhile(ASTWhileNode whil)
        {
            while (Interpret(whil.Expression) != 0)
            {
                PushScope();
                Interpret(whil.Statement);
                PopScope();
                if (isBreak)
                {
                    isBreak = false;
                    break;
                }
                isContinue = false;
            }
        }

        private void InterpretCompound(ASTCompundNode comp)
        {
            PushScope();
            foreach (var blockItem in comp.BlockItems)
                Interpret(blockItem);
            PopScope();
        }

        private int InterpretConditionalExpression(ASTConditionalExpressionNode condEx)
        {
            if (Interpret(condEx.Condition) != 0)
            {
                return Interpret(condEx.IfBranch);
            }
            else
            {
                return Interpret(condEx.ElseBranch);
            }
        }

        private void InterpretCondition(ASTConditionNode cond)
        {
            if (Interpret(cond.Condition) != 0)
            {
                Interpret(cond.IfBranch);
            }
            else
            {
                if (cond.ElseBranch is not ASTNoStatementNode)
                {
                    Interpret(cond.ElseBranch);
                }
            }
        }

        private int InterpretVariable(ASTVariableNode variable)
        {
            if (variable.IsGlobal)
            {
                return globalVariables[variable.Name];
            }
            else
            {
                foreach (var check in variables)
                {
                    if (check.ContainsKey(variable.Name))
                    {
                        return check[variable.Name];
                    }
                }

                return variables.Peek()[variable.Name];
            }
        }

        private int InterpretAssign(ASTAssignNode assign)
        {
            if (assign.IsGlobal)
            {
                return globalVariables[assign.Name] = Interpret(assign.Expression);
            }
            else
            {
                foreach (var check in variables)
                {
                    if (check.ContainsKey(assign.Name))
                    {
                        return check[assign.Name] = Interpret(assign.Expression);
                    }
                }

                return variables.Peek()[assign.Name] = Interpret(assign.Expression);
            }
        }

        private void InterpretDeclaration(ASTDeclarationNode dec)
        {
            if (dec.IsGlobal)
            {
                InterpretGlobalDeclaration(dec);
                return;
            }

            if (dec.Initializer is not ASTNoExpressionNode)
            {
                variables.Peek()[dec.Name] = Interpret(dec.Initializer);
            }
            else
            {
                variables.Peek()[dec.Name] = 0;
            }
        }

        private void InterpretGlobalDeclaration(ASTDeclarationNode dec)
        {
            if (dec.Initializer is ASTConstantNode constant)
            {
                globalVariables[dec.Name] = constant.Value;
            }
            else
            {
                globalVariables[dec.Name] = 0;
            }
        }

        private int InterpretConstant(ASTConstantNode constant)
        {
            return constant.Value;
        }

        private int InterpretUnaryOp(ASTUnaryOpNode unaryOp)
        {
            switch (unaryOp.Value)
            {
                case '-': return -Interpret(unaryOp.Expression);
                case '~': return ~Interpret(unaryOp.Expression);
                case '!': return Interpret(unaryOp.Expression) == 0 ? 1 : 0;
                case '+':
                default: return Interpret(unaryOp.Expression);
            }
        }

        private int InterpretBinaryOp(ASTBinaryOpNode binOp)
        {
            if (binOp.Value == "||")
            {
                if (Interpret(binOp.ExpressionLeft) != 0)
                {
                    return 1;
                }
                else
                {
                    return Interpret(binOp.ExpressionRight) != 0 ? 1 : 0;
                }
            }
            else if (binOp.Value == "&&")
            {
                if (Interpret(binOp.ExpressionLeft) == 0)
                {
                    return 0;
                }
                else
                {
                    return Interpret(binOp.ExpressionRight) != 0 ? 1 : 0;
                }
            }
            else
            {
                if (binOp.IsComparison)
                {
                    switch (binOp.Value)
                    {
                        case "==": return Interpret(binOp.ExpressionLeft) == Interpret(binOp.ExpressionRight) ? 1 : 0;
                        case "!=": return Interpret(binOp.ExpressionLeft) != Interpret(binOp.ExpressionRight) ? 1 : 0;
                        case ">=": return Interpret(binOp.ExpressionLeft) >= Interpret(binOp.ExpressionRight) ? 1 : 0;
                        case ">": return Interpret(binOp.ExpressionLeft) > Interpret(binOp.ExpressionRight) ? 1 : 0;
                        case "<=": return Interpret(binOp.ExpressionLeft) <= Interpret(binOp.ExpressionRight) ? 1 : 0;
                        case "<": return Interpret(binOp.ExpressionLeft) < Interpret(binOp.ExpressionRight) ? 1 : 0;
                    }
                }
                else
                {
                    switch (binOp.Value)
                    {
                        case "+": return Interpret(binOp.ExpressionLeft) + Interpret(binOp.ExpressionRight);
                        case "*": return Interpret(binOp.ExpressionLeft) * Interpret(binOp.ExpressionRight);
                        case "-": return Interpret(binOp.ExpressionLeft) - Interpret(binOp.ExpressionRight);
                        case "<<": return Interpret(binOp.ExpressionLeft) << Interpret(binOp.ExpressionRight);
                        case ">>": return Interpret(binOp.ExpressionLeft) >> Interpret(binOp.ExpressionRight);
                        case "&": return Interpret(binOp.ExpressionLeft) & Interpret(binOp.ExpressionRight);
                        case "|": return Interpret(binOp.ExpressionLeft) | Interpret(binOp.ExpressionRight);
                        case "^": return Interpret(binOp.ExpressionLeft) ^ Interpret(binOp.ExpressionRight);
                        case "/": return Interpret(binOp.ExpressionLeft) / Interpret(binOp.ExpressionRight);
                        case "%": return Interpret(binOp.ExpressionLeft) % Interpret(binOp.ExpressionRight);
                    }
                }

                return 0;
            }
        }

        private void InterpretReturn(ASTReturnNode ret)
        {
            Function function = functions[functionsStack.Peek()];
            function.ReturnValue = Interpret(ret.Expression);
            functions[functionsStack.Peek()] = function;
            functionsStack.Pop();
            hasReturned = true;
        }

        private int InterpretExpression(ASTExpressionNode exp)
        {
            return Interpret(exp.Expression);
        }

        private int InterpretFunction(ASTFunctionNode function)
        {
            if (function.IsDefinition)
            {
                functionsStack.Push(function.Name);
                variables.Push(new Dictionary<string, int>());
                foreach (var arg in functions[function.Name].Parameters)
                {
                    variables.Peek()[arg.Name] = arg.Value;
                }

                foreach (var blockItem in function.BlockItems)
                {
                    Interpret(blockItem);
                    if (blockItem is ASTReturnNode || hasReturned)
                    {
                        break;
                    }
                }
                variables.Pop();
                hasReturned = false;
                return functions[function.Name].ReturnValue;
            }

            return 0;
        }

        private void InterpretProgram(ASTProgramNode program)
        {
            foreach (var topLevelItem in program.TopLevelItems)
            {
                if (topLevelItem is ASTFunctionNode function && function.IsDefinition)
                {
                    Function func = new();
                    func.FunctionNode = function;
                    func.Parameters = new List<Parameter>();
                    foreach (var param in function.Parameters)
                    {
                        Parameter parameter = new();
                        parameter.Name = param;
                        func.Parameters.Add(parameter);
                    }
                    functions[function.Name] = func;
                }

                if (topLevelItem is ASTDeclarationNode decl)
                    Interpret(decl);
            }
        }

        private void PushScope()
        {
            variables.Push(new Dictionary<string, int>());
        }

        private void PopScope()
        {
            variables.Pop();
        }
    }
}
