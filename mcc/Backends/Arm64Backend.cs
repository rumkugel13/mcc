using System.Runtime.InteropServices;
using System.Text;

namespace mcc.Backends
{
    internal class Arm64Backend : IBackend
    {
        StringBuilder sb = new StringBuilder();

        const string lbVarAddress = ".addr_";
        readonly string[] argRegister4B = new string[8] { "w0", "w1", "w2", "w3", "w4", "w5", "w6", "w7", };
        readonly string[] argRegister8B = new string[8] { "x0", "x1", "x2", "x3", "x4", "x5", "x6", "x7", };

        const int pointerSize = 8;

        OSPlatform targetOS;

        public Arm64Backend(OSPlatform os)
        {
            this.targetOS = os;
        }

        public string GetAssembly()
        {
            return sb.ToString();
        }

        public void GenerateGlobalVariable(string name, int value)
        {
            Instruction(".globl " + name);
            Instruction(".data");
            Instruction(".balign 4");
            Label(name);
            Instruction(".word " + value);
        }

        public void GenerateGlobalVariableAddress(string name)
        {
            Label(lbVarAddress + name);
            Instruction(".dword " + name);
        }

        public void GenerateUninitializedGlobalVariable(string name)
        {
            // not defined, add to bss
            Instruction(".globl " + name);
            Instruction(".bss");
            Instruction(".balign 4");
            Label(name);
            Instruction(".zero 4");
        }

        public void FunctionPrologue(string name)
        {
            Instruction(".globl " + name);
            Instruction(".text");
            Label(name);
            Instruction($"stp x29, x30, [sp, #-16]!");    // x29 = frame pointer, x30 = return adress
            Instruction("mov x29, sp");
        }

        public void FunctionEpilogue()
        {
            Instruction("mov sp, x29");
            Instruction($"ldp x29, x30, [sp], #16");
            Instruction("ret");
        }

        public void StoreGlobalVariable(string name)
        {
            Instruction("ldr x2, " + lbVarAddress + name);
            Instruction("str w0, [x2]");
        }

        public void LoadGlobalVariable(string name)
        {
            Instruction("ldr x2, " + lbVarAddress + name);
            Instruction("ldr w0, [x2]");
        }

        public void StoreLocalVariable(int byteOffset)
        {
            Instruction("str w0, [x29, #" + byteOffset + "]");
        }

        public void LoadLocalVariable(int byteOffset)
        {
            Instruction("ldr w0, [x29, #" + byteOffset + "]");
        }

        public void InitializeLocalVariable(int byteOffset)
        {
            Instruction("str wzr, [x29, #" + byteOffset + "]");
        }

        public void StoreInt(int offset)
        {
            Instruction($"str w0, [sp, #{offset}]");
        }

        public void AllocateMemory(int bytesToAllocate)
        {
            Instruction("sub sp, sp, #" + bytesToAllocate);
        }

        public void DeallocateMemory(int bytesToDeallocate)
        {
            Instruction("add sp, sp, #" + bytesToDeallocate);
        }

        public void MoveRegisterToMemory(string register, int offset)
        {
            Instruction($"str {register}, [x29, #" + offset + "]");
        }

        public void MoveMemoryToRegister(string register, int offset)
        {
            Instruction($"ldr {register}, [sp, #{offset}]");
        }

        public int AllocateAtLeast(int bytes)
        {
            // allocate space for arguments, 16 byte aligned
            int allocate = 16 * ((bytes + 15) / 16);
            AllocateMemory(allocate);

            return allocate;
        }

        public void MoveArgsIntoRegisters(int argCount)
        {
            int regsUsed = Math.Min(argCount, argRegister4B.Length);

            // move arguments into registers
            for (int i = 0; i < regsUsed; i++)
            {
                MoveMemoryToRegister(argRegister4B[i], i * pointerSize);
            }
        }

        public void MoveRegistersIntoMemory(int argCount)
        {
            int regsUsed = Math.Min(argCount, argRegister4B.Length);

            for (int i = 0; i < regsUsed; i++)
            {
                MoveRegisterToMemory(argRegister4B[i], -(i + 1) * 4);
            }
        }

        public void PreCallDeallocate(int allocated, int argCount)
        {
            int regsUsed = Math.Min(argCount, argRegister4B.Length);

            if (argCount > regsUsed)
            {
                // pre deallocate temp memory, so that args on memory are in correct offset
                DeallocateMemory(regsUsed * pointerSize);
            }
            else
            {
                // deallocate all temp memory, since all args are in registers
                DeallocateMemory(allocated);
            }
        }

        public void PostCallDeallocate(int allocated, int argCount)
        {
            int regsUsed = Math.Min(argCount, argRegister4B.Length);

            if (argCount > regsUsed)
            {
                // post deallocate temp memory, we dont ned args on memory anymore
                DeallocateMemory(allocated - regsUsed * pointerSize);
            }
        }

        public void CallFunction(string name)
        {
            Instruction("bl " + name);
        }

        public void PushLeftOperand()
        {
            Instruction("str w0, [sp, #-16]!");   // push 16 bytes, needs to be 16 byte aligned
        }

        public void PopLeftOperand()
        {
            Instruction("ldr w1, [sp], #16");     // pop 16 bytes
        }

        public void CompareZero()
        {
            Instruction("cmp w0, #0");
        }

        public void SetIfNotEqual()
        {
            Instruction("cset w0, ne");
        }

        public void Jump(string label)
        {
            Instruction("b " + label);
        }

        public void JumpEqual(string label)
        {
            Instruction("b.eq " + label);
        }

        public void JumpNotEqual(string label)
        {
            Instruction("b.ne " + label);
        }

        public void IntegerConstant(int value)
        {
            if (value < 65536)
            {
                Instruction("mov w0, #" + value);
            }
            else
            {
                Instruction("mov w0, #" + (value & 0xFFFF));        // lower 16 bits
                Instruction("movk w0, #" + (value >> 16) + ", lsl 16");  // upper 16 bits, shifted by 16 bits without modifying register bits
            }
        }

        public void ComparisonOperation(string op)
        {
            Instruction("cmp w1, w0");

            switch (op)
            {
                case "==": Instruction("cset w0, eq"); break;
                case "!=": Instruction("cset w0, ne"); break;
                case ">=": Instruction("cset w0, ge"); break;
                case ">": Instruction("cset w0, gt"); break;
                case "<=": Instruction("cset w0, le"); break;
                case "<": Instruction("cset w0, lt"); break;
            }
        }

        public void BinaryOperation(string op)
        {
            switch (op)
            {
                case "+": Instruction("add w0, w1, w0"); break;
                case "*": Instruction("mul w0, w1, w0"); break;
                case "-": Instruction("sub w0, w1, w0"); break;
                case "<<": Instruction("lsl w0, w1, w0"); break;
                case ">>": Instruction("asr w0, w1, w0"); break;
                case "&": Instruction("and w0, w1, w0"); break;
                case "|": Instruction("orr w0, w1, w0"); break;
                case "^": Instruction("eor w0, w1, w0"); break;
                case "/":
                    Instruction("sdiv w0, w1, w0");
                    break;
                case "%":
                    Instruction("sdiv w2, w1, w0");
                    Instruction("msub w0, w2, w0, w1");
                    break;
            }
        }

        public void UnaryOperation(char op)
        {
            switch (op)
            {
                case '+': break;    // just for completeness
                case '-': Instruction("neg w0, w0"); break;
                case '~': Instruction("mvn w0, w0"); break;
                case '!':
                    Instruction("cmp w0, #0");
                    Instruction("cset w0, eq");
                    break;
            }
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
