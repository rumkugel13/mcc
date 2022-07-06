using System.Text;

namespace mcc
{
    class Generator
    {
        StringBuilder sb = new StringBuilder();
        Dictionary<string, int> varMap = new Dictionary<string, int>();
        const int varSize = 8; // 32bit = 4, 64bit = 8
        int varOffset = -varSize;

        public void Label(string label)
        {
            sb.AppendLine("_" + label + ":");
        }

        public void Instruction(string instruction)
        {
            sb.AppendLine("\t" + instruction);
        }

        public void IntegerConstant(int value)
        {
            Instruction("movq $" + value + ", %rax");
        }

        public void ReferenceVariable(string variable)
        {
            if (varMap.TryGetValue(variable, out int offset))
            {
                Instruction("movq " + offset + "(%rbp), %rax");
            }
            else
                throw new ASTVariableException("Trying to reference a non existing Variable: " + variable);
        }

        public void AssignVariable(string variable)
        {
            if (varMap.TryGetValue(variable, out int offset))
            {
                Instruction("movq %rax, " + offset + "(%rbp)");
            }
            else
                throw new ASTVariableException("Trying to assign to non existing Variable: " + variable);
        }

        public void DeclareVariable(string variable)
        {
            if (varMap.ContainsKey(variable))
                throw new ASTVariableException("Trying to declare existing Variable: " + variable);

            Instruction("pushq %rax"); // push current value of variable to stack
            varMap.Add(variable, varOffset);
            varOffset -= varSize;
        }

        public void FunctionPrologue(string label)
        {
            sb.AppendLine(label + ":");
            Instruction("pushq %rbp");
            Instruction("movq %rsp, %rbp");
        }

        public void FunctionEpilogue()
        {
            Instruction("movq %rbp, %rsp");
            Instruction("popq %rbp");
            Instruction("ret");
        }

        public string CreateOutput()
        {
            return sb.ToString();
        }
    }
}
