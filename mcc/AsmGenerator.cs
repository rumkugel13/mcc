using mcc.Backends;

namespace mcc
{
    class AsmGenerator
    {
        ASTNode rootNode;
        IBackend backend;

        int labelCounter = 0;
        string functionScope;
        const string lbLoopBegin = ".lb";
        const string lbLoopContinue = ".lc";
        const string lbLoopPost = ".lp";
        const string lbLoopEnd = ".le";
        const string lbJump = ".j";
        const string lbJumpEqual = ".je";
        const string lbJumpNotEqual = ".jne";
        const string lbEndFunction = ".end_";

        const int pointerSize = 8;

        public AsmGenerator(ASTNode rootNode)
        {
            this.rootNode = rootNode;
        }

        public string GenerateX64()
        {
            backend = new X64Backend();
            Generate(rootNode);
            return backend.GetAssembly();
        }

        public string GenerateArm64()
        {
            backend = new Arm64Backend();
            Generate(rootNode);
            return backend.GetAssembly();
        }

        private void Generate(ASTNode node)
        {
            switch (node)
            {
                case ASTNoExpressionNode: break;
                case ASTNoStatementNode: break;
                case ASTProgramNode program: GenerateProgram(program); break;
                case ASTFunctionNode function: GenerateFunction(function); break;
                case ASTReturnNode ret: GenerateReturn(ret); break;
                case ASTConstantNode constant: GenerateConstant(constant); break;
                case ASTUnaryOpNode unOp: GenerateUnaryOp(unOp); break;
                case ASTBinaryOpNode binOp: GenerateBinaryOp(binOp); break;
                case ASTExpressionNode exp: GenerateExpression(exp); break;
                case ASTDeclarationNode dec: GenerateDeclaration(dec); break;
                case ASTAssignNode assign: GenerateAssign(assign); break;
                case ASTVariableNode variable: GenerateVariable(variable); break;
                case ASTConditionNode cond: GenerateCondition(cond); break;
                case ASTConditionalExpressionNode condEx: GenerateConditionalExpression(condEx); break;
                case ASTCompundNode comp: GenerateCompound(comp); break;
                case ASTWhileNode whil: GenerateWhile(whil); break;
                case ASTDoWhileNode doWhil: GenerateDoWhile(doWhil); break;
                case ASTForNode fo: GenerateFor(fo); break;
                case ASTForDeclarationNode forDecl: GenerateForDeclaration(forDecl); break;
                case ASTBreakNode br: GenerateBreak(br); break;
                case ASTContinueNode con: GenerateContinue(con); break;
                case ASTFunctionCallNode funCall: GenerateFunctionCall(funCall); break;
                default: throw new NotImplementedException("Unkown ASTNode type: " + node.GetType());
            }
        }

        private void GenerateFunctionCall(ASTFunctionCallNode funCall)
        {
            // allocate space for arguments, 16 byte aligned
            int allocated = backend.AllocateAtLeast(funCall.Arguments.Count * pointerSize);

            // move arguments beginning at last argument, up the stack beginning at stack pointer into temp storage
            for (int i = funCall.Arguments.Count - 1; i >= 0; i--)
            {
                Generate(funCall.Arguments[i]);
                backend.StoreInt(i * pointerSize);
            }

            // move arguments into registers
            backend.MoveArgsIntoRegisters(funCall.Arguments.Count);

            // pre deallocate temp memory, so that args on memory are in correct offset
            backend.PreCallDeallocate(allocated, funCall.Arguments.Count);

            backend.CallFunction(funCall.Name);

            // post deallocate temp memory, we dont ned args on memory anymore
            backend.PostCallDeallocate(allocated, funCall.Arguments.Count);
        }

        private void GenerateContinue(ASTContinueNode con)
        {
            backend.Jump(lbLoopContinue + con.LoopCount);
        }

        private void GenerateBreak(ASTBreakNode br)
        {
            backend.Jump(lbLoopEnd + br.LoopCount);
        }

        private void GenerateForDeclaration(ASTForDeclarationNode forDecl)
        {
            Generate(forDecl.Declaration);
            backend.Jump(lbLoopPost + forDecl.LoopCount);
            backend.Label(lbLoopBegin + forDecl.LoopCount);
            Generate(forDecl.Statement);
            backend.Label(lbLoopContinue + forDecl.LoopCount);
            Deallocate(forDecl.BytesToDeallocate);
            Generate(forDecl.Post);
            backend.Label(lbLoopPost + forDecl.LoopCount);
            Generate(forDecl.Condition);
            backend.CompareZero();
            backend.JumpNotEqual(lbLoopBegin + forDecl.LoopCount);
            backend.Label(lbLoopEnd + forDecl.LoopCount);
            Deallocate(forDecl.BytesToDeallocateInit);
        }

        private void GenerateFor(ASTForNode fo)
        {
            Generate(fo.Init);
            backend.Jump(lbLoopPost + fo.LoopCount);
            backend.Label(lbLoopBegin + fo.LoopCount);
            Generate(fo.Statement);
            backend.Label(lbLoopContinue + fo.LoopCount);
            Deallocate(fo.BytesToDeallocate);
            Generate(fo.Post);
            backend.Label(lbLoopPost + fo.LoopCount);
            Generate(fo.Condition);
            backend.CompareZero();
            backend.JumpNotEqual(lbLoopBegin + fo.LoopCount);
            backend.Label(lbLoopEnd + fo.LoopCount);
            Deallocate(fo.BytesToDeallocateInit);
        }

        private void GenerateDoWhile(ASTDoWhileNode doWhil)
        {
            backend.Label(lbLoopBegin + doWhil.LoopCount);
            Generate(doWhil.Statement);
            backend.Label(lbLoopContinue + doWhil.LoopCount);
            Deallocate(doWhil.BytesToDeallocate);
            Generate(doWhil.Expression);
            backend.CompareZero();
            backend.JumpNotEqual(lbLoopBegin + doWhil.LoopCount);
            backend.Label(lbLoopEnd + doWhil.LoopCount);
        }

        private void GenerateWhile(ASTWhileNode whil)
        {
            backend.Jump(lbLoopContinue + whil.LoopCount);
            backend.Label(lbLoopBegin + whil.LoopCount);
            Generate(whil.Statement);
            backend.Label(lbLoopContinue + whil.LoopCount);
            Deallocate(whil.BytesToDeallocate);
            Generate(whil.Expression);
            backend.CompareZero();
            backend.JumpNotEqual(lbLoopBegin + whil.LoopCount);
            backend.Label(lbLoopEnd + whil.LoopCount);
        }

        private void GenerateCompound(ASTCompundNode comp)
        {
            foreach (var blockItem in comp.BlockItems)
                Generate(blockItem);

            Deallocate(comp.BytesToDeallocate);
        }

        private void Deallocate(int bytes)
        {
            //Instruction($"addq ${bytes}, %rsp"); // pop off variables from current scope
        }

        private void GenerateConditionalExpression(ASTConditionalExpressionNode condEx)
        {
            Generate(condEx.Condition);
            backend.CompareZero();
            string label = CreateJumpLabel(lbJumpEqual);
            backend.JumpEqual(label);
            Generate(condEx.IfBranch);
            string end = CreateJumpLabel(lbJump);
            backend.Jump(end);
            backend.Label(label);
            Generate(condEx.ElseBranch);
            backend.Label(end);
        }

        private void GenerateCondition(ASTConditionNode cond)
        {
            Generate(cond.Condition);
            backend.CompareZero();
            string label = CreateJumpLabel(lbJumpEqual);
            backend.JumpEqual(label);

            if (cond.ElseBranch is not ASTNoStatementNode)
            {
                Generate(cond.IfBranch);
                string end = CreateJumpLabel(lbJump);
                backend.Jump(end);
                backend.Label(label);
                Generate(cond.ElseBranch);
                backend.Label(end);
            }
            else
            {
                Generate(cond.IfBranch);
                backend.Label(label);
            }
        }

        private void GenerateVariable(ASTVariableNode variable)
        {
            if (variable.IsGlobal)
            {
                backend.LoadGlobalVariable(variable.Name);
            }
            else
            {
                backend.LoadLocalVariable(variable.Offset);
            }
        }

        private void GenerateAssign(ASTAssignNode assign)
        {
            Generate(assign.Expression);
            if (assign.IsGlobal)
            {
                backend.StoreGlobalVariable(assign.Name);
            }
            else
            {
                backend.StoreLocalVariable(assign.Offset);
            }
        }

        private void GenerateDeclaration(ASTDeclarationNode dec)
        {
            if (dec.IsGlobal)
            {
                GenerateGlobalDeclaration(dec);
                return;
            }

            if (dec.Initializer is not ASTNoExpressionNode)
            {
                Generate(dec.Initializer);
                backend.StoreLocalVariable(dec.Offset);
            }
            else
            {
                backend.InitializeLocalVariable(dec.Offset);
            }
        }

        private void GenerateGlobalDeclaration(ASTDeclarationNode dec)
        {
            if (dec.Initializer is not ASTNoExpressionNode)
            {
                backend.GenerateGlobalVariable(dec.Name, dec.GlobalValue);
            }
        }

        private void GenerateConstant(ASTConstantNode constant)
        {
            backend.IntegerConstant(constant.Value);
        }

        private void GenerateUnaryOp(ASTUnaryOpNode unaryOp)
        {
            Generate(unaryOp.Expression);
            backend.UnaryOperation(unaryOp.Value);
        }

        private void GenerateShortCircuit(ASTBinaryOpNode binOp)
        {
            Generate(binOp.ExpressionLeft);
            backend.CompareZero();
            string jumpEqualOrNotLabel = "";
            if (binOp.Value == "||")
            {
                jumpEqualOrNotLabel = CreateJumpLabel(lbJumpEqual);
                backend.JumpEqual(jumpEqualOrNotLabel);
                backend.IntegerConstant(1);
            }
            else if (binOp.Value == "&&")
            {
                jumpEqualOrNotLabel = CreateJumpLabel(lbJumpNotEqual);
                backend.JumpNotEqual(jumpEqualOrNotLabel);
            }

            string endLabel = CreateJumpLabel(lbJump);
            backend.Jump(endLabel);
            backend.Label(jumpEqualOrNotLabel);
            Generate(binOp.ExpressionRight);
            backend.CompareZero();
            backend.SetIfNotEqual();
            backend.Label(endLabel);
        }

        private void GenerateBinaryOp(ASTBinaryOpNode binOp)
        {
            if (binOp.NeedsShortCircuit)
            {
                GenerateShortCircuit(binOp);
                return;
            }

            Generate(binOp.ExpressionLeft);
            backend.PushLeftOperand();
            Generate(binOp.ExpressionRight);
            backend.PopLeftOperand();

            if (binOp.IsComparison)
            {
                backend.ComparisonOperation(binOp.Value);
            }
            else
            {
                backend.BinaryOperation(binOp.Value);
            }
        }

        private void GenerateReturn(ASTReturnNode ret)
        {
            Generate(ret.Expression);
            if (ret.IsLastReturn)
            {
                backend.Label(lbEndFunction + functionScope);
                backend.FunctionEpilogue();
            }
            else
            {
                backend.Jump(lbEndFunction + functionScope);
            }
        }

        private void GenerateExpression(ASTExpressionNode exp)
        {
            Generate(exp.Expression);
        }

        private void GenerateFunction(ASTFunctionNode function)
        {
            if (function.IsDefinition)
            {
                functionScope = function.Name;
                backend.FunctionPrologue(function.Name);
                backend.AllocateMemory(function.BytesToAllocate);

                backend.MoveRegistersIntoMemory(function.Parameters.Count);

                foreach (var blockItem in function.BlockItems)
                    Generate(blockItem);

                if (!function.ContainsReturn)
                {
                    // return 0 if no return statement found
                    backend.IntegerConstant(0);
                    backend.Label(lbEndFunction + function.Name);
                    backend.FunctionEpilogue();
                }
            }
        }

        private void GenerateProgram(ASTProgramNode program)
        {
            foreach (var topLevelItem in program.TopLevelItems)
                Generate(topLevelItem);

            foreach (var variable in program.GlobalVariables)
                backend.GenerateGlobalVariableAddress(variable);

            foreach (var variable in program.UninitializedGlobalVariables)
                backend.GenerateUninitializedGlobalVariable(variable);
        }

        private string CreateJumpLabel(string name)
        {
            return name + labelCounter++;
        }
    }
}
