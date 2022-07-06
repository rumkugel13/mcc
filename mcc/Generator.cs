using System.Text;

namespace mcc
{
    class Generator
    {
        StringBuilder sb = new StringBuilder();

        const int varSize = 8; // 32bit = 4, 64bit = 8
        int varOffset = -varSize;
        int labelCounter = 0;

        int currentScope = -1;
        List<Dictionary<string, int>> varMapList = new List<Dictionary<string, int>>();
        List<HashSet<string>> varScopeList = new List<HashSet<string>>();

        public void Label(string label)
        {
            sb.AppendLine(label + ":");
        }

        public void Instruction(string instruction)
        {
            sb.AppendLine("\t" + instruction);
        }

        public void IntegerConstant(int value)
        {
            Instruction("movq $" + value + ", %rax");
        }

        public void CompareZero()
        {
            Instruction("cmpq $0, %rax");
        }

        public string Jump()
        {
            string jmpLabel = "_jmp" + labelCounter++;
            Instruction("jmp " + jmpLabel);
            return jmpLabel;
        }

        public string JumpEqual()
        {
            string jmpLabel = "_je" + labelCounter++;
            Instruction("je " + jmpLabel);
            return jmpLabel;
        }

        public string JumpNotEqual()
        {
            string jmpLabel = "_jne" + labelCounter++;
            Instruction("jne " + jmpLabel);
            return jmpLabel;
        }

        public void UnaryOperation(char op)
        {
            switch (op)
            {
                case '+': break;    // just for completeness
                case '-': Instruction("negq %rax"); break;
                case '~': Instruction("notq %rax"); break;
                case '!':
                    Instruction("cmpq $0, %rax");
                    Instruction("movq $0, %rax");
                    Instruction("sete %al");
                    break;
            }
        }

        public void BinaryOperation(string op)
        {
            switch (op)
            {
                case "+": Instruction("addq %rcx, %rax"); break;
                case "*": Instruction("imulq %rcx, %rax"); break;
                case "-": Instruction("subq %rcx, %rax"); break;
                case "<<": Instruction("sal %rcx, %rax"); break;
                case ">>": Instruction("sar %rcx, %rax"); break;
                case "&": Instruction("and %rcx, %rax"); break;
                case "|": Instruction("or %rcx, %rax"); break;
                case "^": Instruction("xor %rcx, %rax"); break;
                case "/":
                    Instruction("cdq");
                    Instruction("idivl %ecx");
                    break;
                case "%":
                   Instruction("cdq");
                   Instruction("idivl %ecx");
                   Instruction("movq %rdx, %rax");
                    break;
            }
        }

        public void ComparisonOperation(string op)
        {
            Instruction("cmpq %rcx, %rax");
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

        public void StartBlock()
        {
            currentScope++;
            Dictionary<string, int> varMap;
            HashSet<string> varScope = new HashSet<string>();

            varMap = currentScope > 0 ? new Dictionary<string, int>(varMapList[currentScope - 1]) : new Dictionary<string, int>();

            varMapList.Add(varMap);
            varScopeList.Add(varScope);
        }

        public void EndBlock()
        {
            int newVarCount = varScopeList[currentScope].Count;
            varMapList.RemoveAt(currentScope);
            varScopeList.RemoveAt(currentScope);

            varOffset += newVarCount * varSize;
            Instruction($"addq ${newVarCount * varSize}, %rsp"); // pop off variables from current scope
            currentScope--;
        }

        public void ReferenceVariable(string variable)
        {
            if (varMapList[currentScope].TryGetValue(variable, out int offset))
            {
                Instruction("movq " + offset + "(%rbp), %rax");
            }
            else
                throw new ASTVariableException("Trying to reference a non existing Variable: " + variable);
        }

        public void AssignVariable(string variable)
        {
            if (varMapList[currentScope].TryGetValue(variable, out int offset))
            {
                Instruction("movq %rax, " + offset + "(%rbp)");
            }
            else
                throw new ASTVariableException("Trying to assign to non existing Variable: " + variable);
        }

        public void DeclareVariable(string variable)
        {
            if (varScopeList[currentScope].Contains(variable))
                throw new ASTVariableException("Trying to declare existing Variable: " + variable);

            Instruction("pushq %rax"); // push current value of variable to stack
            varMapList[currentScope][variable] = varOffset; // add or update variable offset
            varScopeList[currentScope].Add(variable);
            varOffset -= varSize;
        }

        public void FunctionPrologue(string label)
        {
            Label(label);
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
