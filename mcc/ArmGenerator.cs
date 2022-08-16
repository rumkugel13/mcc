using System.Text;

namespace mcc
{
    internal class ArmGenerator
    {
        ASTNode rootNode;
        StringBuilder sb = new StringBuilder();
        int varLabelCounter = 0;
        int bytesAllocated = 0;

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
                ArmInstruction("str w0, [sp, #-16]!");   // push 16 bytes, needs to be 16 byte aligned
            }

            for (int i = 0; i < funCall.Arguments.Count; i++)
            {
                ArmInstruction($"ldr w{i}, [sp], #16");     // pop 16 bytes into correct register
            }
            // note: only works for first 8 arguments, afterwards unexpected behaviour

            ArmInstruction("bl " + funCall.Name);
            //Instruction("add $" + funCall.BytesToDeallocate + ", %rsp");
        }

        private void GenerateContinue(ASTContinueNode con)
        {
            ArmInstruction("b loop_continue_" + con.LoopCount);
        }

        private void GenerateBreak(ASTBreakNode br)
        {
            ArmInstruction("b loop_end_" + br.LoopCount);
        }

        private void GenerateForDeclaration(ASTForDeclarationNode forDecl)
        {
            Generate(forDecl.Declaration);
            ArmInstruction("b loop_post_" + forDecl.LoopCount);
            Label("loop_begin_" + forDecl.LoopCount);
            Generate(forDecl.Statement);
            Label("loop_continue_" + forDecl.LoopCount);
            Deallocate(forDecl.BytesToDeallocate);
            Generate(forDecl.Post);
            Label("loop_post_" + forDecl.LoopCount);
            Generate(forDecl.Condition);
            CompareZero();
            ArmInstruction("b.ne loop_begin_" + forDecl.LoopCount);
            Label("loop_end_" + forDecl.LoopCount);
            Deallocate(forDecl.BytesToDeallocateInit);
        }

        private void GenerateFor(ASTForNode fo)
        {
            Generate(fo.Init);
            ArmInstruction("b loop_post_" + fo.LoopCount);
            Label("loop_begin_" + fo.LoopCount);
            Generate(fo.Statement);
            Label("loop_continue_" + fo.LoopCount);
            Deallocate(fo.BytesToDeallocate);
            Generate(fo.Post);
            Label("loop_post_" + fo.LoopCount);
            Generate(fo.Condition);
            CompareZero();
            ArmInstruction("b.ne loop_begin_" + fo.LoopCount);
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
            ArmInstruction("b.ne loop_begin_" + doWhil.LoopCount);
            Label("loop_end_" + doWhil.LoopCount);
        }

        private void GenerateWhile(ASTWhileNode whil)
        {
            ArmInstruction("b loop_continue_" + whil.LoopCount);
            Label("loop_begin_" + whil.LoopCount);
            Generate(whil.Statement);
            Label("loop_continue_" + whil.LoopCount);
            Deallocate(whil.BytesToDeallocate);
            Generate(whil.Expression);
            CompareZero();
            ArmInstruction("b.ne loop_begin_" + whil.LoopCount);
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
            //Instruction($"addq ${bytes}, %rsp"); // pop off variables from current scope
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
                ArmInstruction("ldr x0, addr_for_" + variable.Name);
                ArmInstruction("ldr w0, [x0]");
            }
            else
            {
                ArmInstruction("ldr w0, [x29, #" + variable.Offset + "]");
            }
        }

        private void GenerateAssign(ASTAssignNode assign)
        {
            Generate(assign.Expression);
            if (assign.IsGlobal)
            {
                ArmInstruction("ldr x0, addr_for_" + assign.Name);
                ArmInstruction("str w0, [x0]");
            }
            else
            {
                ArmInstruction("str w0, [x29, #" + assign.Offset + "]");
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
                ArmInstruction("str w0, [x29, #" + dec.Offset + "]");
            }
            else
            {
                ArmInstruction("str wzr, [x29, #" + dec.Offset + "]");
            }
        }

        private void GenerateGlobalDeclaration(ASTDeclarationNode dec)
        {
            if (dec.Initializer is not ASTNoExpressionNode)
            {
                ArmInstruction(".globl " + dec.Name);
                ArmInstruction(".data");
                ArmInstruction(".balign 4");
                Label(dec.Name);
                ArmInstruction(".word " + dec.GlobalValue);
            }
        }

        private void GenerateGlobalVariableAddress(string name)
        {
            Label("addr_for_" + name);
            ArmInstruction(".dword " + name);
        }

        private void GenerateUninitializedGlobalVariable(string name)
        {
            // not defined, add to bss
            ArmInstruction(".globl " + name);
            ArmInstruction(".bss");
            ArmInstruction(".balign 4");
            Label(name);
            ArmInstruction(".zero 4");
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
            ArmInstruction("cset w0, ne");
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
            ArmInstruction("str w0, [sp, #-16]!");   // push 16 bytes, needs to be 16 byte aligned
            Generate(binOp.ExpressionRight);
            ArmInstruction("ldr w1, [sp], #16");     // pop 16 bytes

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
                bytesAllocated = function.BytesToAllocate;
                FunctionPrologue(function.Name);

                for (int i = 0; i < function.Parameters.Count; i++)
                {
                    // move arguments from registers to reserved stack position
                    ArmInstruction($"str w{i}, [x29, #" + (-(i + 1) * 4) + "]");
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

        private void FunctionPrologue(string name)
        {
            ArmInstruction(".globl " + name);
            ArmInstruction(".text");
            Label(name);
            ArmInstruction($"stp x29, x30, [sp, #-{16 + bytesAllocated}]!");    // x29 = frame pointer, x30 = return adress
            ArmInstruction("mov x29, sp");
        }

        private void FunctionEpilogue()
        {
            ArmInstruction($"ldp x29, x30, [sp], #{16 + bytesAllocated}");
            ArmInstruction("ret");
        }

        private void GenerateProgram(ASTProgramNode program)
        {
            foreach (var topLevelItem in program.TopLevelItems)
                Generate(topLevelItem);

            foreach (var variable in program.GlobalVariables)
                GenerateGlobalVariableAddress(variable);

            foreach (var variable in program.UninitializedGlobalVariables)
                GenerateUninitializedGlobalVariable(variable);
        }

        private void CompareZero()
        {
            ArmInstruction("cmp w0, #0");
        }

        private string Jump()
        {
            string jmpLabel = "_b" + varLabelCounter++;
            ArmInstruction("b " + jmpLabel);
            return jmpLabel;
        }

        private string JumpEqual()
        {
            string jmpLabel = "_b.eq" + varLabelCounter++;
            ArmInstruction("b.eq " + jmpLabel);
            return jmpLabel;
        }

        private string JumpNotEqual()
        {
            string jmpLabel = "_b.ne" + varLabelCounter++;
            ArmInstruction("b.ne " + jmpLabel);
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
                ArmInstruction("mov w0, #" + (value & 0xFFFF));        // lower 16 bits
                ArmInstruction("movk w0, #" + (value >> 16) + ", lsl 16");  // upper 16 bits, shifted by 16 bits without modifying register bits
            }
        }

        private void ComparisonOperation(string op)
        {
            ArmInstruction("cmp w1, w0");

            switch (op)
            {
                case "==": ArmInstruction("cset w0, eq"); break;
                case "!=": ArmInstruction("cset w0, ne"); break;
                case ">=": ArmInstruction("cset w0, ge"); break;
                case ">":  ArmInstruction("cset w0, gt"); break;
                case "<=": ArmInstruction("cset w0, le"); break;
                case "<":  ArmInstruction("cset w0, lt"); break;
            }
        }

        private void BinaryOperation(string op)
        {
            switch (op)
            {
                case "+": ArmInstruction("add w0, w1, w0"); break;
                case "*": ArmInstruction("mul w0, w1, w0"); break;
                case "-": ArmInstruction("sub w0, w1, w0"); break;
                case "<<": ArmInstruction("lsl w0, w1, w0"); break;
                case ">>": ArmInstruction("asr w0, w1, w0"); break;
                case "&": ArmInstruction("and w0, w1, w0"); break;
                case "|": ArmInstruction("orr w0, w1, w0"); break;
                case "^": ArmInstruction("eor w0, w1, w0"); break;
                case "/":
                    ArmInstruction("sdiv w0, w1, w0");
                    break;
                case "%":
                    ArmInstruction("sdiv w2, w1, w0");
                    ArmInstruction("msub w0, w2, w0, w1");
                    break;
            }
        }

        private void Label(string label)
        {
            sb.AppendLine(label + ":");
        }

        private void ArmInstruction(string instruction)
        {
            sb.AppendLine("\t" + instruction);
        }
    }
}
