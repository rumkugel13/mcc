using System.Text;

namespace mcc
{
    internal class ArmGenerator
    {
        ASTNode rootNode;
        StringBuilder sb = new StringBuilder();
        int varLabelCounter = 0;

        public ArmGenerator(ASTNode rootNode)
        {
            this.rootNode = rootNode;
        }

        public string GenerateARM()
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
            var list = new List<ASTAbstractExpressionNode>(funCall.Arguments);
            list.Reverse();
            foreach (var exp in list)
            {
                Generate(exp);
                Instruction("push %rax");
            }

            // HACK: workaround for hello_world, expects first parameter to be in another register
            if (OperatingSystem.IsLinux())
            {
                Instruction("movl %eax, %edi"); // rdi (System V AMD64 ABI)
            }
            else
            {
                Instruction("movl %eax, %ecx"); // rcx (MS x64 calling convention)
            }

            Instruction("call " + funCall.Name);
            Instruction("add $" + funCall.BytesToDeallocate + ", %rsp");
        }

        private void GenerateContinue(ASTContinueNode con)
        {
            Instruction("jmp loop_continue_" + con.LoopCount);
        }

        private void GenerateBreak(ASTBreakNode br)
        {
            Instruction("jmp loop_end_" + br.LoopCount);
        }

        private void GenerateForDeclaration(ASTForDeclarationNode forDecl)
        {
            Generate(forDecl.Declaration);
            Label("loop_begin_" + forDecl.LoopCount);
            Generate(forDecl.Condition);
            CompareZero();
            Instruction("je loop_end_" + forDecl.LoopCount);
            Generate(forDecl.Statement);
            Label("loop_continue_" + forDecl.LoopCount);
            Deallocate(forDecl.BytesToDeallocate);
            Generate(forDecl.Post);
            Instruction("jmp loop_begin_" + forDecl.LoopCount);
            Label("loop_end_" + forDecl.LoopCount);
            Deallocate(forDecl.BytesToDeallocateInit);
        }

        private void GenerateFor(ASTForNode fo)
        {
            Generate(fo.Init);
            Label("loop_begin_" + fo.LoopCount);
            Generate(fo.Condition);
            CompareZero();
            Instruction("je loop_end_" + fo.LoopCount);
            Generate(fo.Statement);
            Label("loop_continue_" + fo.LoopCount);
            Deallocate(fo.BytesToDeallocate);
            Generate(fo.Post);
            Instruction("jmp loop_begin_" + fo.LoopCount);
            Label("loop_end_" + fo.LoopCount);
            Deallocate(fo.BytesToDeallocateInit);
        }

        private void GenerateDoWhile(ASTDoWhileNode doWhil)
        {
            Label("loop_begin_" + doWhil.LoopCount);
            Generate(doWhil.Statement);
            Label("loop_continue_" + doWhil.LoopCount);
            Deallocate(doWhil.BytesToDeallocate);
            Generate(doWhil.Expression);
            CompareZero();
            Instruction("jne loop_begin_" + doWhil.LoopCount);
            Label("loop_end_" + doWhil.LoopCount);
        }

        private void GenerateWhile(ASTWhileNode whil)
        {
            Label("loop_begin_" + whil.LoopCount);
            Generate(whil.Expression);
            CompareZero();
            Instruction("je loop_end_" + whil.LoopCount);
            Generate(whil.Statement);
            Label("loop_continue_" + whil.LoopCount);
            Deallocate(whil.BytesToDeallocate);
            Instruction("jmp loop_begin_" + whil.LoopCount);
            Label("loop_end_" + whil.LoopCount);
        }

        private void GenerateCompound(ASTCompundNode comp)
        {
            foreach (var blockItem in comp.BlockItems)
                Generate(blockItem);

            Deallocate(comp.BytesToDeallocate);
        }

        private void Deallocate(int bytes)
        {
            Instruction($"addq ${bytes}, %rsp"); // pop off variables from current scope
        }

        private void GenerateConditionalExpression(ASTConditionalExpressionNode condEx)
        {
            Generate(condEx.Condition);
            CompareZero();
            string label = JumpEqual();
            Generate(condEx.IfBranch);
            string end = Jump();
            Label(label);
            Generate(condEx.ElseBranch);
            Label(end);
        }

        private void GenerateCondition(ASTConditionNode cond)
        {
            Generate(cond.Condition);
            CompareZero();
            string label = JumpEqual();

            if (cond.ElseBranch is not ASTNoStatementNode)
            {
                Generate(cond.IfBranch);
                string end = Jump();
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
                Instruction("movl " + variable.Name + "(%rip), %eax");
            }
            else
            {
                Instruction("movl " + variable.Offset + "(%rbp), %eax");
            }
        }

        private void GenerateAssign(ASTAssignNode assign)
        {
            Generate(assign.Expression);
            if (assign.IsGlobal)
            {
                Instruction("movl %eax, " + assign.Name + "(%rip)");
            }
            else
            {
                Instruction("movl %eax, " + assign.Offset + "(%rbp)");
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
            }
            else
            {
                IntegerConstant(0); // no value given, assign 0
            }

            Instruction("push %rax"); // push current value of variable to stack
        }

        private void GenerateGlobalDeclaration(ASTDeclarationNode dec)
        {
            if (dec.Initializer is not ASTNoExpressionNode)
            {
                Instruction(".globl " + dec.Name);
                Instruction(".data");
                Instruction(".align 4");
                Label(dec.Name);
                Instruction(".long " + dec.GlobalValue);
            }
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

        private void GenerateConstant(ASTConstantNode constant)
        {
            IntegerConstant(constant.Value);
        }

        private void GenerateUnaryOp(ASTUnaryOpNode unaryOp)
        {
            Generate(unaryOp.Expression);
            switch (unaryOp.Value)
            {
                case '+': break;    // just for completeness
                case '-': ArmInstruction("neg w0, w0"); break;
                case '~': ArmInstruction("mvn w0, w0"); break;
                case '!':
                    ArmInstruction("cmp w0, #0");
                    ArmInstruction("cset w0, eq");
                    break;
            }
        }

        private void GenerateShortCircuit(ASTBinaryOpNode binOp)
        {
            Generate(binOp.ExpressionLeft);
            CompareZero();
            string jumpEqualOrNotLabel = "";
            if (binOp.Value == "||")
            {
                jumpEqualOrNotLabel = JumpEqual();
                IntegerConstant(1);
            }
            else if (binOp.Value == "&&")
            {
                jumpEqualOrNotLabel = JumpNotEqual();
            }

            string endLabel = Jump();
            Label(jumpEqualOrNotLabel);
            Generate(binOp.ExpressionRight);
            CompareZero();
            IntegerConstant(0);
            Instruction("setne %al");
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
            Instruction("push %rax");
            Generate(binOp.ExpressionRight);
            Instruction("movl %eax, %ecx"); // need to switch src and dest for - and /
            Instruction("pop %rax");

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

        private void FunctionPrologue(string name)
        {
            ArmInstruction(".globl " + name);
            ArmInstruction(".text");
            Label(name);
            //Instruction("pushq %rbp");
            //Instruction("movq %rsp, %rbp");
        }

        private void FunctionEpilogue()
        {
            //Instruction("movq %rbp, %rsp");
            //Instruction("popq %rbp");
            ArmInstruction("ret");
        }

        private void GenerateProgram(ASTProgramNode program)
        {
            foreach (var topLevelItem in program.TopLevelItems)
                Generate(topLevelItem);

            foreach (var variable in program.UninitializedGlobalVariables)
                GenerateUninitializedGlobalVariable(variable);
        }

        private void CompareZero()
        {
            Instruction("cmpl $0, %eax");
        }

        private string Jump()
        {
            string jmpLabel = "_jmp" + varLabelCounter++;
            Instruction("jmp " + jmpLabel);
            return jmpLabel;
        }

        private string JumpEqual()
        {
            string jmpLabel = "_je" + varLabelCounter++;
            Instruction("je " + jmpLabel);
            return jmpLabel;
        }

        private string JumpNotEqual()
        {
            string jmpLabel = "_jne" + varLabelCounter++;
            Instruction("jne " + jmpLabel);
            return jmpLabel;
        }

        private void IntegerConstant(int value)
        {
            if (value < 65536)
            {
                ArmInstruction("mov w0, #" + value);
            }
            else
            {
                ArmInstruction("mov w0, #" + ((value << 16) >> 16));        // lower 16 bits
                ArmInstruction("movk w0, #" + (value >> 16) + ", lsl 16");  // upper 16 bits, shifted by 16 bits without modifying register bits
            }
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

        private void Label(string label)
        {
            sb.AppendLine(label + ":");
        }

        private void Instruction(string instruction)
        {
            // do not print x86 instructions
            //sb.AppendLine("\t" + instruction);
        }

        private void ArmInstruction(string instruction)
        {
            sb.AppendLine("\t" + instruction);
        }
    }
}
