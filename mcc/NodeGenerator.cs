using System.Text;

namespace mcc
{
    class NodeGenerator
    {
        ASTNode rootNode;
        StringBuilder sb = new StringBuilder();
        int varLabelCounter = 0;

        public NodeGenerator(ASTNode rootNode)
        {
            this.rootNode = rootNode;
        }

        public void Generate(ASTNode node)
        {
            switch (node)
            {
                case ASTProgramNode program: GenerateProgramNode(program); break;
                case ASTFunctionNode function: GenerateFunctionNode(function); break;
                case ASTReturnNode ret: GenerateReturnNode(ret); break;
                case ASTConstantNode constant: GenerateConstantNode(constant); break;
                case ASTUnaryOpNode unOp: GenerateUnaryOpNode(unOp); break;
                case ASTBinaryOpNode binOp: GenerateBinaryOpNode(binOp); break;
                case ASTExpressionNode exp: GenerateExpressionNode(exp); break;
                case ASTDeclarationNode dec: GenerateDeclarationNode(dec); break;
                case ASTAssignNode assign: GenerateAssignNode(assign); break;
                case ASTVariableNode variable: GenerateVariableNode(variable); break;
                default: Console.WriteLine("Fail: Unkown ASTNode type: " + node.GetType()); break;
            }
        }

        private void GenerateVariableNode(ASTVariableNode variable)
        {
            Instruction("movl " + variable.Offset + "(%rbp), %eax");
        }

        private void GenerateAssignNode(ASTAssignNode assign)
        {
            Generate(assign.Expression);
            Instruction("movl %eax, " + assign.Offset + "(%rbp)");
        }

        private void GenerateDeclarationNode(ASTDeclarationNode dec)
        {
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

        private void GenerateConstantNode(ASTConstantNode constant)
        {
            Instruction("movl $" + constant.Value + ", %eax");
        }

        private void GenerateUnaryOpNode(ASTUnaryOpNode unaryOp)
        {
            Generate(unaryOp.Expression);
            switch (unaryOp.Value)
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

        private void GenerateBinaryOpNode(ASTBinaryOpNode binOp)
        {
            if (Symbol2.ShortCircuit.Contains(binOp.Value))
            {
                GenerateShortCircuit(binOp);
                return;
            }

            Generate(binOp.ExpressionLeft);
            Instruction("push %rax");
            Generate(binOp.ExpressionRight);
            Instruction("movl %eax, %ecx"); // need to switch src and dest for - and /
            Instruction("pop %rax");

            if (Symbol2.Comparison.Contains(binOp.Value))
            {
                ComparisonOperation(binOp.Value);
            }
            else
            {
                BinaryOperation(binOp.Value);
            }
        }

        private void GenerateReturnNode(ASTReturnNode ret)
        {
            Generate(ret.Expression);
            FunctionEpilogue();
        }

        private void GenerateExpressionNode(ASTExpressionNode exp)
        {
            Generate(exp.Expression);
        }

        private void GenerateFunctionNode(ASTFunctionNode function)
        {
            FunctionPrologue(function.Name);

            foreach (var statement in function.Statements)
                Generate(statement);


        }

        private void FunctionPrologue(string name)
        {
            Instruction(".globl " + name);
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

        private void GenerateProgramNode(ASTProgramNode program)
        {
            GenerateFunctionNode(program.Function);
        }

        public string GenerateX86()
        {
            Generate(rootNode);
            return sb.ToString();
        }

        public void Label(string label)
        {
            sb.AppendLine(label + ":");
        }

        public void CompareZero()
        {
            Instruction("cmpl $0, %eax");
        }

        public string Jump()
        {
            string jmpLabel = "_jmp" + varLabelCounter++;
            Instruction("jmp " + jmpLabel);
            return jmpLabel;
        }

        public string JumpEqual()
        {
            string jmpLabel = "_je" + varLabelCounter++;
            Instruction("je " + jmpLabel);
            return jmpLabel;
        }

        public string JumpNotEqual()
        {
            string jmpLabel = "_jne" + varLabelCounter++;
            Instruction("jne " + jmpLabel);
            return jmpLabel;
        }

        public void IntegerConstant(int value)
        {
            //if (varMaps.Count == 0)
            //{
            //    DefineGlobalVariable(value);
            //    return;
            //}

            Instruction("movl $" + value + ", %eax");
        }

        public void ComparisonOperation(string op)
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

        public void BinaryOperation(string op)
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

        public void Instruction(string instruction)
        {
            sb.AppendLine("\t" + instruction);
        }
    }
}
