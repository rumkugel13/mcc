using System.Text;

namespace mcc.Backends
{
    internal class BytecodeBackend : IBackend
    {
        readonly StringBuilder sb = new StringBuilder();
        int memoryOffset = 0;
        Dictionary<int, int> varOffsets = new();

        const int intSize = 4;

        public int AllocateAtLeast(int bytes)
        {
            //throw new NotImplementedException();
            return 0;
        }

        public void AllocateMemory(int bytesToAllocate)
        {
            //throw new NotImplementedException();
        }

        public int GetArgCountNotInRegs(int argCount)
        {
            return argCount;
        }

        public int AllocateVariable(int index, int size)
        {
            int offset = memoryOffset;
            varOffsets[index] = offset;
            memoryOffset += size;
            return offset;
        }

        public int GetVariableLocation(int index)
        {
            return varOffsets[index];
        }

        public void BinaryOperation(string op)
        {
            switch (op)
            {
                case "+": Instruction("addi"); break;
                case "*": Instruction("muli"); break;
                case "-": Instruction("subi"); break;
                case "<<": Instruction("shli"); break;
                case ">>": Instruction("sari"); break;
                case "&": Instruction("andi"); break;
                case "|": Instruction("ori"); break;
                case "^": Instruction("xori"); break;
                case "/": Instruction("divi"); break;
                case "%": Instruction("remi"); break;
            }
        }

        public void CallFunction(string name)
        {
            Instruction("call " + name);
        }

        public void CompareZero()
        {
            Instruction("cmp_ze");
        }

        public void ComparisonOperation(string op)
        {
            switch (op)
            {
                case "==": Instruction("cmp_eq"); break;
                case "!=": Instruction("cmp_neq"); break;
                case ">=": Instruction("cmp_ge"); break;
                case ">": Instruction("cmp_gt"); break;
                case "<=": Instruction("cmp_le"); break;
                case "<": Instruction("cmp_lt"); break;
            }
        }

        public void DeallocateMemory(int bytesToDeallocate)
        {
            //throw new NotImplementedException();
        }

        public void DropValue()
        {
            Instruction("dropi");
        }

        public void FunctionEpilogue()
        {
            Instruction("ret");
        }

        public void FunctionPrologue(string name)
        {
            memoryOffset = 0;
            varOffsets.Clear();
            Instruction(".text");
            Label(name);
        }

        public void GenerateGlobalVariable(string name, int val)
        {
            Instruction(".data");
            Label(name);
            Instruction(".int " + val);
        }

        public void GenerateGlobalVariableAddress(string name)
        {
            //throw new NotImplementedException();
        }

        public void GenerateUninitializedGlobalVariable(string name)
        {
            // refactor: put this in bss when supported
            Instruction(".data");
            Label(name);
            Instruction(".int " + 0);
        }

        public string GetAssembly()
        {
            return sb.ToString();
        }

        public void InitializeLocalVariable(int byteOffset)
        {
            Instruction("immi 0");
            Instruction("storei " + byteOffset);
        }

        public void Instruction(string instruction)
        {
            sb.AppendLine("\t" + instruction);
        }

        public void IntegerConstant(int value)
        {
            Instruction("immi " + value);
        }

        public void Jump(string label)
        {
            Instruction("jmp " + label);
        }

        public void JumpEqual(string label)
        {
            Instruction("jmp_nz " + label); // note: we compare to zero beforehand, so maybe this might be incorrect
        }

        public void JumpNotEqual(string label)
        {
            Instruction("jmp_z " + label);
        }

        public void Label(string label)
        {
            sb.AppendLine(":" + label);
        }

        public void LoadGlobalVariable(string name)
        {
            Instruction("loadgi " + name);
        }

        public void LoadLocalVariable(int byteOffset)
        {
            Instruction("loadi " + byteOffset);
        }

        public void MoveArgsIntoRegisters(int argCount)
        {
            for (int i = argCount - 1; i >= 0; i--)
            {
                Instruction("loadi " + (i * intSize));
            }
        }

        public void MoveMemoryToRegister(string register, int offset)
        {
            //throw new NotImplementedException();
        }

        public void MoveRegistersIntoMemory(int argCount)
        {
            for (int i = 0; i < argCount; i++)
            {
                Instruction("storei " + memoryOffset);
                varOffsets[i] = memoryOffset;
                memoryOffset += intSize;
            }
        }

        public void MoveRegisterToMemory(string register, int offset)
        {
            //throw new NotImplementedException();
        }

        public void PopLeftOperand()
        {
            //throw new NotImplementedException();
        }

        public void PostCallDeallocate(int allocated, int argCount)
        {
            //throw new NotImplementedException();
        }

        public void PreCallDeallocate(int allocated, int argCount)
        {
            //throw new NotImplementedException();
        }

        public void PushLeftOperand()
        {
            //throw new NotImplementedException();
        }

        public void SetIfNotEqual()
        {
            // todo: examine
            Instruction("cmp_ze");
        }

        public void StoreGlobalVariable(string name)
        {
            Instruction("storegi " + name);
        }

        public void StoreArgInStack(int index, int size)
        {
            Instruction("storei " + (index * size));
        }

        public void StoreLocalVariable(int byteOffset, bool keepValue = false)
        {
            if (keepValue)
                Instruction("dupi");
            Instruction("storei " + byteOffset);
        }

        public void UnaryOperation(char op)
        {
            switch (op)
            {
                case '+': break;    // just for completeness
                case '-': Instruction("negi"); break;
                case '~': Instruction("noti"); break;
                case '!': Instruction("lnoti"); break;
            }
        }
    }
}
