using System.Text;

namespace mcc
{
    class Generator
    {
        StringBuilder sb = new StringBuilder();

        const int varSize = 8; // 32bit = 4, 64bit = 8
        int varOffset = -varSize;
        int varLabelCounter = 0;

        Stack<Dictionary<string, int>> varMaps = new Stack<Dictionary<string, int>>();
        Stack<HashSet<string>> varScopes = new Stack<HashSet<string>>();

        int loopLabelCounter = 0;
        Stack<int> loops = new Stack<int>();

        struct Function
        {
            public int ParameterCount;
            public bool Defined;
        }

        Dictionary<string, Function> funcMap = new Dictionary<string, Function>();

        const int paramOffset = 2 * varSize;
        int paramCount = 0;

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

        public int LoopBeginLabel()
        {
            Label("loop_begin" + loopLabelCounter);
            loops.Push(loopLabelCounter);
            return loopLabelCounter++;
        }

        public void LoopContinueLabel(int count)
        {
            Label("loop_continue" + count);
        }

        public void LoopEndLabel(int count)
        {
            Label("loop_end" + count);
            loops.Pop();
        }

        public void LoopJumpEqualEnd(int count)
        {
            Instruction("je loop_end" + count);
        }

        public void LoopJumpNotEqualBegin(int count)
        {
            Instruction("jne loop_begin" + count);
        }

        public void LoopJumpBegin(int count)
        {
            Instruction("jmp loop_begin" + count);
        }

        public void LoopJumpEnd(int count)
        {
            Instruction("jmp loop_end" + count);
        }

        public void LoopJumpContinue(int count)
        {
            Instruction("jmp loop_continue" + count);
        }

        public void BeginLoopBlock()
        {
            BeginBlock();
        }

        public void EndLoopBlock()
        {
            EndBlock();
        }

        public void LoopBreak()
        {
            if (loops.Count == 0)
                throw new ASTLoopScopeException("Fail: Can't break out of non existing loop scope");
            LoopJumpEnd(loops.Peek());
        }

        public void LoopContinue()
        {
            if (loops.Count == 0)
                throw new ASTLoopScopeException("Fail: Can't continue in non existing loop scope");
            LoopJumpContinue(loops.Peek());
        }

        public void BeginBlock()
        {
            Dictionary<string, int> varMap;
            HashSet<string> varScope = new HashSet<string>();

            varMap = this.varMaps.Count > 0 ? new Dictionary<string, int>(varMaps.Peek()) : new Dictionary<string, int>();
            varMaps.Push(varMap);
            varScopes.Push(varScope);
        }

        public void EndBlock()
        {
            int newVarCount = varScopes.Peek().Count - paramCount; // exclude variables from arguments
            varMaps.Pop();
            varScopes.Pop();

            varOffset += newVarCount * varSize;
            Instruction($"addq ${newVarCount * varSize}, %rsp"); // pop off variables from current scope
        }

        public void ReferenceVariable(string variable)
        {
            if (varMaps.Peek().TryGetValue(variable, out int offset))
            {
                Instruction("movq " + offset + "(%rbp), %rax");
            }
            else
                throw new ASTVariableException("Fail: Trying to reference a non existing Variable: " + variable);
        }

        public void AssignVariable(string variable)
        {
            if (varMaps.Peek().TryGetValue(variable, out int offset))
            {
                Instruction("movq %rax, " + offset + "(%rbp)");
            }
            else
                throw new ASTVariableException("Fail: Trying to assign to non existing Variable: " + variable);
        }

        public void DeclareVariable(string variable)
        {
            if (varScopes.Peek().Contains(variable))
                throw new ASTVariableException("Fail: Trying to declare existing Variable: " + variable);

            Instruction("pushq %rax"); // push current value of variable to stack
            varMaps.Peek()[variable] = varOffset; // add or update variable offset
            varScopes.Peek().Add(variable);
            varOffset -= varSize;
        }

        public void DeclareParameter(string variable)
        {
            varMaps.Peek()[variable] = paramOffset + paramCount * varSize;
            varScopes.Peek().Add(variable);
            paramCount++;
        }

        public void DeclareFunction(string label, int parameterCount)
        {
            if (funcMap.TryGetValue(label, out Function function))
            {
                if (function.ParameterCount != parameterCount)
                    throw new ASTFunctionException("Fail: Trying to declare already existing function");
            }
            else
            {
                funcMap.Add(label, new Function() { Defined = false, ParameterCount = parameterCount });
            }
        }

        public void FunctionPrologue(string label, int parameterCount)
        {
            if (funcMap.TryGetValue(label, out Function function))
            {
                if (function.Defined)
                    throw new ASTFunctionException("Fail: Trying to define already existing function");
                else if (function.ParameterCount != parameterCount)
                    throw new ASTFunctionException("Fail: Trying to define declared function with wrong parameter count");
                else funcMap[label] = new Function() { Defined = true, ParameterCount = parameterCount };
            }
            else 
                funcMap.Add(label, new Function() { Defined = true, ParameterCount = parameterCount });

            paramCount = 0;
            varOffset = -varSize;
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

        public void CallFunction(string function, int arguments)
        {
            if (funcMap.TryGetValue(function, out Function value))
            {
                if (value.ParameterCount != arguments)
                    throw new ASTFunctionException("Fail: Trying to call function with too little/many parameters");
            }
            else
                throw new ASTFunctionException("Fail: Trying to call non existing function");

            Instruction("call " + function);
        }

        public void RemoveArguments(int count)
        {
            Instruction("addq $" + count * varSize + ", %rsp");
        }

        public string CreateOutput()
        {
            return sb.ToString();
        }
    }
}
