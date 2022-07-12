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
                case ASTProgramNode program:
                    GenerateProgramNode(program);
                    break;
                case ASTFunctionNode function:
                    GenerateFunctionNode(function);
                    break;
                case ASTReturnNode ret:
                    GenerateReturnNode(ret);
                    break;
                case ASTConstantNode constant:
                    GenerateConstantNode(constant);
                    break;
            }
        }

        private void GenerateConstantNode(ASTConstantNode constant)
        {
            Instruction("movl $" + constant.Value + ", %eax");
        }

        private void GenerateReturnNode(ASTReturnNode ret)
        {
            GenerateConstantNode(ret.Constant);
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
