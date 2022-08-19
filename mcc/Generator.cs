using System.Text;

namespace mcc
{
    class Generator
    {
        ASTNode rootNode;
        StringBuilder sb = new StringBuilder();
        int varLabelCounter = 0;
        const string lbLoopBegin = ".lb";
        const string lbLoopContinue = ".lc";
        const string lbLoopPost = ".lp";
        const string lbLoopEnd = ".le";
        const string lbJump = ".j";
        const string lbJumpEqual = ".je";
        const string lbJumpNotEqual = ".jne";
        readonly string[] argRegister4B = new string[6] { "edi", "esi", "edx", "ecx", "r8d", "r9d", };
        readonly string[] argRegister8B = new string[6] { "rdi", "rsi", "rdx", "rcx", "r8", "r9", };
        readonly string[] argRegisterWin4B = new string[4] { "ecx", "edx", "r8d", "r9d", };
        readonly string[] argRegisterWin8B = new string[4] { "rcx", "rdx", "r8", "r9", };

        public Generator(ASTNode rootNode)
        {
            this.rootNode = rootNode;
        }

        public string GenerateX86()
        {
            Generate(rootNode);
            return sb.ToString();
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
            for (int i = funCall.Arguments.Count - 1; i >= 0; i--)
            {
                Generate(funCall.Arguments[i]);
                Instruction("pushq %rax");
            }

            // todo: parameters beyond 4/6 are on the stack (need stack position calculations)
            if (OperatingSystem.IsLinux())
            {
                for (int i = 0; i < Math.Min(funCall.Arguments.Count, argRegister8B.Length); i++)
                {
                    Instruction("popq %" + argRegister8B[i]);
                }
            }
            else
            {
                for (int i = 0; i < Math.Min(funCall.Arguments.Count, argRegisterWin8B.Length); i++)
                {
                    Instruction("popq %" + argRegisterWin8B[i]);
                }
            }

            CallFunction(funCall.Name);
            //DeallocateMemory(funCall.BytesToDeallocate);
        }

        private void GenerateContinue(ASTContinueNode con)
        {
            Jump(lbLoopContinue + con.LoopCount);
        }

        private void GenerateBreak(ASTBreakNode br)
        {
            Jump(lbLoopEnd + br.LoopCount);
        }

        private void GenerateForDeclaration(ASTForDeclarationNode forDecl)
        {
            Generate(forDecl.Declaration);
            Jump(lbLoopPost + forDecl.LoopCount);
            Label(lbLoopBegin + forDecl.LoopCount);
            Generate(forDecl.Statement);
            Label(lbLoopContinue + forDecl.LoopCount);
            Deallocate(forDecl.BytesToDeallocate);
            Generate(forDecl.Post);
            Label(lbLoopPost + forDecl.LoopCount);
            Generate(forDecl.Condition);
            CompareZero();
            JumpNotEqual(lbLoopBegin + forDecl.LoopCount);
            Label(lbLoopEnd + forDecl.LoopCount);
            Deallocate(forDecl.BytesToDeallocateInit);
        }

        private void GenerateFor(ASTForNode fo)
        {
            Generate(fo.Init);
            Jump(lbLoopPost + fo.LoopCount);
            Label(lbLoopBegin + fo.LoopCount);
            Generate(fo.Statement);
            Label(lbLoopContinue + fo.LoopCount);
            Deallocate(fo.BytesToDeallocate);
            Generate(fo.Post);
            Label(lbLoopPost + fo.LoopCount);
            Generate(fo.Condition);
            CompareZero();
            JumpNotEqual(lbLoopBegin + fo.LoopCount);
            Label(lbLoopEnd + fo.LoopCount);
            Deallocate(fo.BytesToDeallocateInit);
        }

        private void GenerateDoWhile(ASTDoWhileNode doWhil)
        {
            Label(lbLoopBegin + doWhil.LoopCount);
            Generate(doWhil.Statement);
            Label(lbLoopContinue + doWhil.LoopCount);
            Deallocate(doWhil.BytesToDeallocate);
            Generate(doWhil.Expression);
            CompareZero();
            JumpNotEqual(lbLoopBegin + doWhil.LoopCount);
            Label(lbLoopEnd + doWhil.LoopCount);
        }

        private void GenerateWhile(ASTWhileNode whil)
        {
            Jump(lbLoopContinue + whil.LoopCount);
            Label(lbLoopBegin + whil.LoopCount);
            Generate(whil.Statement);
            Label(lbLoopContinue + whil.LoopCount);
            Deallocate(whil.BytesToDeallocate);
            Generate(whil.Expression);
            CompareZero();
            JumpNotEqual(lbLoopBegin + whil.LoopCount);
            Label(lbLoopEnd + whil.LoopCount);
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
            CompareZero();
            string label = CreateJumpLabel(lbJumpEqual);
            JumpEqual(label);
            Generate(condEx.IfBranch);
            string end = CreateJumpLabel(lbJump);
            Jump(end);
            Label(label);
            Generate(condEx.ElseBranch);
            Label(end);
        }

        private void GenerateCondition(ASTConditionNode cond)
        {
            Generate(cond.Condition);
            CompareZero();
            string label = CreateJumpLabel(lbJumpEqual);
            JumpEqual(label);

            if (cond.ElseBranch is not ASTNoStatementNode)
            {
                Generate(cond.IfBranch);
                string end = CreateJumpLabel(lbJump);
                Jump(end);
                Label(label);
                Generate(cond.ElseBranch);
                Label(end);
            }
            else
            {
                Generate(cond.IfBranch);
                Label(label);
            }
        }

        private void GenerateVariable(ASTVariableNode variable)
        {
            if (variable.IsGlobal)
            {
                LoadGlobalVariable(variable.Name);
            }
            else
            {
                LoadLocalVariable(variable.Offset);
            }
        }

        private void GenerateAssign(ASTAssignNode assign)
        {
            Generate(assign.Expression);
            if (assign.IsGlobal)
            {
                StoreGlobalVariable(assign.Name);
            }
            else
            {
                StoreLocalVariable(assign.Offset);
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
                StoreLocalVariable(dec.Offset);
            }
            else
            {
                InitializeLocalVariable(dec.Offset);
            }
        }

        private void GenerateGlobalDeclaration(ASTDeclarationNode dec)
        {
            if (dec.Initializer is not ASTNoExpressionNode)
            {
                GenerateGlobalVariable(dec.Name, dec.GlobalValue);
            }
        }

        private void GenerateConstant(ASTConstantNode constant)
        {
            IntegerConstant(constant.Value);
        }

        private void GenerateUnaryOp(ASTUnaryOpNode unaryOp)
        {
            Generate(unaryOp.Expression);
            UnaryOperation(unaryOp.Value);
        }

        private void GenerateShortCircuit(ASTBinaryOpNode binOp)
        {
            Generate(binOp.ExpressionLeft);
            CompareZero();
            string jumpEqualOrNotLabel = "";
            if (binOp.Value == "||")
            {
                jumpEqualOrNotLabel = CreateJumpLabel(lbJumpEqual);
                JumpEqual(jumpEqualOrNotLabel);
                IntegerConstant(1);
            }
            else if (binOp.Value == "&&")
            {
                jumpEqualOrNotLabel = CreateJumpLabel(lbJumpNotEqual);
                JumpNotEqual(jumpEqualOrNotLabel);
            }

            string endLabel = CreateJumpLabel(lbJump);
            Jump(endLabel);
            Label(jumpEqualOrNotLabel);
            Generate(binOp.ExpressionRight);
            CompareZero();
            SetIfNotEqual();
            Label(endLabel);
        }

        private void GenerateBinaryOp(ASTBinaryOpNode binOp)
        {
            if (binOp.NeedsShortCircuit)
            {
                GenerateShortCircuit(binOp);
                return;
            }

            Generate(binOp.ExpressionLeft);
            PushLeftOperand();
            Generate(binOp.ExpressionRight);
            PopLeftOperand();

            if (binOp.IsComparison)
            {
                ComparisonOperation(binOp.Value);
            }
            else
            {
                BinaryOperation(binOp.Value);
            }
        }

        private void GenerateReturn(ASTReturnNode ret)
        {
            Generate(ret.Expression);
            FunctionEpilogue();
        }

        private void GenerateExpression(ASTExpressionNode exp)
        {
            Generate(exp.Expression);
        }

        private void GenerateFunction(ASTFunctionNode function)
        {
            if (function.IsDefinition)
            {
                FunctionPrologue(function.Name);
                AllocateMemoryForVariables(function.BytesToAllocate);

                // todo: parameters beyond 4/6 are on the stack (need stack position calculations)
                if (OperatingSystem.IsLinux())
                {
                    for (int i = 0; i < Math.Min(function.Parameters.Count, argRegister4B.Length); i++)
                    {
                        Instruction("movl %" + argRegister4B[i] + ", " + (-(i + 1) * 4) + "(%rbp)");
                    }
                }
                else
                {
                    for (int i = 0; i < Math.Min(function.Parameters.Count, argRegisterWin4B.Length); i++)
                    {
                        Instruction("movl %" + argRegisterWin4B[i] + ", " + (-(i + 1) * 4) + "(%rbp)");
                    }
                }

                foreach (var blockItem in function.BlockItems)
                    Generate(blockItem);

                if (!function.ContainsReturn)
                {
                    // return 0 if no return statement found
                    IntegerConstant(0);
                    FunctionEpilogue();
                }
            }
        }

        private void GenerateProgram(ASTProgramNode program)
        {
            foreach (var topLevelItem in program.TopLevelItems)
                Generate(topLevelItem);

            foreach (var variable in program.UninitializedGlobalVariables)
                GenerateUninitializedGlobalVariable(variable);
        }

        private void GenerateGlobalVariable(string name, int value)
        {
            Instruction(".globl " + name);
            Instruction(".data");
            Instruction(".align 4");
            Label(name);
            Instruction(".long " + value);
        }

        private void GenerateUninitializedGlobalVariable(string name)
        {
            // not defined, add to bss
            Instruction(".globl " + name);
            Instruction(".bss");
            Instruction(".align 4");
            Label(name);
            Instruction(".zero 4");
        }

        private void FunctionPrologue(string name)
        {
            Instruction(".globl " + name);
            Instruction(".text");
            Label(name);
            Instruction("pushq %rbp");
            Instruction("movq %rsp, %rbp");
        }

        private void FunctionEpilogue()
        {
            Instruction("movq %rbp, %rsp");
            Instruction("popq %rbp");
            Instruction("ret");
        }

        private void StoreGlobalVariable(string name)
        {
            Instruction("movl %eax, " + name + "(%rip)");
        }

        private void LoadGlobalVariable(string name)
        {
            Instruction("movl " + name + "(%rip), %eax");
        }

        private void StoreLocalVariable(int byteOffset)
        {
            Instruction("movl %eax, " + byteOffset + "(%rbp)");
        }

        private void LoadLocalVariable(int byteOffset)
        {
            Instruction("movl " + byteOffset + "(%rbp), %eax");
        }

        private void InitializeLocalVariable(int byteOffset)
        {
            IntegerConstant(0); // no value given, assign 0
            StoreLocalVariable(byteOffset);
        }

        private void AllocateMemoryForVariables(int bytesToAllocate)
        {
            Instruction("subq $" + bytesToAllocate + ", %rsp");
        }

        private void DeallocateMemory(int bytesToDeallocate)
        {
            Instruction("addq $" + bytesToDeallocate + ", %rsp");
        }

        private void CallFunction(string name)
        {
            Instruction("call " + name);
        }

        private void PushLeftOperand()
        {
            Instruction("push %rax");
        }

        private void PopLeftOperand()
        {
            Instruction("movl %eax, %ecx"); // need to switch src and dest for - and /
            Instruction("pop %rax");
        }

        private void CompareZero()
        {
            Instruction("cmpl $0, %eax");
        }

        private void SetIfNotEqual()
        {
            IntegerConstant(0); // zero out eax
            Instruction("setne %al");
        }

        private string CreateJumpLabel(string name)
        {
            return name + varLabelCounter++;
        }

        private void Jump(string label)
        {
            Instruction("jmp " + label);
        }

        private void JumpEqual(string label)
        {
            Instruction("je " + label);
        }

        private void JumpNotEqual(string label)
        {
            Instruction("jne " + label);
        }

        private void IntegerConstant(int value)
        {
            Instruction("movl $" + value + ", %eax");
        }

        private void ComparisonOperation(string op)
        {
            Instruction("cmpl %ecx, %eax");
            IntegerConstant(0);

            switch (op)
            {
                case "==": Instruction("sete %al"); break;
                case "!=": Instruction("setne %al"); break;
                case ">=": Instruction("setge %al"); break;
                case ">": Instruction("setg %al"); break;
                case "<=": Instruction("setle %al"); break;
                case "<": Instruction("setl %al"); break;
            }
        }

        private void BinaryOperation(string op)
        {
            switch (op)
            {
                case "+": Instruction("addl %ecx, %eax"); break;
                case "*": Instruction("imull %ecx, %eax"); break;
                case "-": Instruction("subl %ecx, %eax"); break;
                case "<<": Instruction("sall %ecx, %eax"); break;
                case ">>": Instruction("sarl %ecx, %eax"); break;
                case "&": Instruction("andl %ecx, %eax"); break;
                case "|": Instruction("orl %ecx, %eax"); break;
                case "^": Instruction("xorl %ecx, %eax"); break;
                case "/":
                    Instruction("cdq");
                    Instruction("idivl %ecx");
                    break;
                case "%":
                    Instruction("cdq");
                    Instruction("idivl %ecx");
                    Instruction("movl %edx, %eax");
                    break;
            }
        }

        private void UnaryOperation(char op)
        {
            switch (op)
            {
                case '+': break;    // just for completeness
                case '-': Instruction("negl %eax"); break;
                case '~': Instruction("notl %eax"); break;
                case '!':
                    Instruction("cmpl $0, %eax");
                    Instruction("movl $0, %eax");
                    Instruction("sete %al");
                    break;
            }
        }

        private void Label(string label)
        {
            sb.AppendLine(label + ":");
        }

        private void Instruction(string instruction)
        {
            sb.AppendLine("\t" + instruction);
        }
    }
}
