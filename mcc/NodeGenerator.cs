using System.Text;

namespace mcc
{
    class NodeGenerator
    {
        ASTNode rootNode;
        StringBuilder sb = new StringBuilder();

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
            }
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

        private void GenerateBinaryOpNode(ASTBinaryOpNode binOp)
        {
            Generate(binOp.ExpressionLeft);
            Instruction("push %rax");
            Generate(binOp.ExpressionRight);
            Instruction("movl %eax, %ecx"); // need to switch src and dest for - and /
            Instruction("pop %rax");

            switch (binOp.Value.ToString())
            {
                case "+": Instruction("addl %ecx, %eax"); break;
                case "*": Instruction("imull %ecx, %eax"); break;
                case "-": Instruction("subl %ecx, %eax"); break;
                //case "<<": Instruction("sall %ecx, %eax"); break;
                //case ">>": Instruction("sarl %ecx, %eax"); break;
                //case "&": Instruction("andl %ecx, %eax"); break;
                //case "|": Instruction("orl %ecx, %eax"); break;
                //case "^": Instruction("xorl %ecx, %eax"); break;
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

        private void GenerateReturnNode(ASTReturnNode ret)
        {
            Generate(ret.Expression);
            Instruction("ret");
        }

        private void GenerateFunctionNode(ASTFunctionNode function)
        {
            Instruction(".globl " + function.Name);
            Label(function.Name);
            GenerateReturnNode(function.Return);
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

        public void Instruction(string instruction)
        {
            sb.AppendLine("\t" + instruction);
        }
    }
}
