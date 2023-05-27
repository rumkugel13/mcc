using System.Runtime.InteropServices;
using System.Text;

namespace mcc.Backends
{
    internal class X64Backend : IBackend
    {
        StringBuilder sb = new StringBuilder();

        readonly string[] argRegister4B = new string[6] { "edi", "esi", "edx", "ecx", "r8d", "r9d", };
        readonly string[] argRegister8B = new string[6] { "rdi", "rsi", "rdx", "rcx", "r8", "r9", };
        readonly string[] argRegisterWin4B = new string[4] { "ecx", "edx", "r8d", "r9d", };
        readonly string[] argRegisterWin8B = new string[4] { "rcx", "rdx", "r8", "r9", };

        int pushCounter = 0;
        const int pointerSize = 8;

        OSPlatform targetOS;

        public X64Backend(OSPlatform os)
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
            Instruction(".align 4");
            Label(name);
            Instruction(".long " + value);
        }

        public void GenerateGlobalVariableAddress(string name)
        {

        }

        public void GenerateUninitializedGlobalVariable(string name)
        {
            // not defined, add to bss
            Instruction(".globl " + name);
            Instruction(".bss");
            Instruction(".align 4");
            Label(name);
            Instruction(".zero 4");
        }

        public void FunctionPrologue(string name)
        {
            Instruction(".globl " + name);
            Instruction(".text");
            Label(name);
            Instruction("pushq %rbp");
            Instruction("movq %rsp, %rbp");
        }

        public void FunctionEpilogue()
        {
            Instruction("movq %rbp, %rsp");
            Instruction("popq %rbp");
            Instruction("ret");
        }

        public void StoreGlobalVariable(string name)
        {
            Instruction("movl %eax, " + name + "(%rip)");
        }

        public void LoadGlobalVariable(string name)
        {
            Instruction("movl " + name + "(%rip), %eax");
        }

        public void StoreLocalVariable(int byteOffset)
        {
            Instruction("movl %eax, " + byteOffset + "(%rbp)");
        }

        public void LoadLocalVariable(int byteOffset)
        {
            Instruction("movl " + byteOffset + "(%rbp), %eax");
        }

        public void InitializeLocalVariable(int byteOffset)
        {
            IntegerConstant(0); // no value given, assign 0
            StoreLocalVariable(byteOffset);
        }

        public void StoreInt(int offset)
        {
            Instruction($"movl %eax, {offset}(%rsp)");
        }

        public void AllocateMemory(int bytesToAllocate)
        {
            Instruction("subq $" + bytesToAllocate + ", %rsp");
        }

        public void DeallocateMemory(int bytesToDeallocate)
        {
            Instruction("addq $" + bytesToDeallocate + ", %rsp");
        }

        public void MoveRegisterToMemory(string register, int offset)
        {
            Instruction("movl %" + register + ", " + offset + "(%rbp)");
        }

        public void MoveMemoryToRegister(string register, int offset)
        {
            Instruction($"movl {offset}(%rsp), %{register}");
        }

        public int AllocateAtLeast(int bytes)
        {
            // allocate space for arguments, 16 byte aligned
            int allocate = IBackend.Align(bytes, 16);
            if (pushCounter % 2 != 0)
            {
                // stack pointer is not aligned (due to binOp), add padding
                allocate += pointerSize;
            }

            // make sure we allocate at least enough for shadow space on windows
            if (targetOS == OSPlatform.Windows)
            {
                allocate = Math.Max(allocate, 4 * pointerSize);
            }

            AllocateMemory(allocate);

            return allocate;
        }

        public void MoveArgsIntoRegisters(int argCount)
        {
            string[] argRegs4 = targetOS == OSPlatform.Linux ? argRegister4B : argRegisterWin4B;
            //string[] argRegs = targetOS == OSPlatform.Linux ? argRegister8B : argRegisterWin8B;
            int regsUsed = Math.Min(argCount, argRegs4.Length);

            // move arguments into registers
            for (int i = 0; i < regsUsed; i++)
            {
                MoveMemoryToRegister(argRegs4[i], i * pointerSize);
            }
        }

        public void MoveRegistersIntoMemory(int argCount)
        {
            string[] argRegs4 = targetOS == OSPlatform.Linux ? argRegister4B : argRegisterWin4B;
            //string[] argRegs = targetOS == OSPlatform.Linux ? argRegister8B : argRegisterWin8B;
            int regsUsed = Math.Min(argCount, argRegs4.Length);

            for (int i = 0; i < regsUsed; i++)
            {
                MoveRegisterToMemory(argRegs4[i], -(i + 1) * 4);
            }
        }

        public void PreCallDeallocate(int allocated, int argCount)
        {
            string[] argRegs4 = targetOS == OSPlatform.Linux ? argRegister4B : argRegisterWin4B;
            //string[] argRegs = targetOS == OSPlatform.Linux ? argRegister8B : argRegisterWin8B;
            int regsUsed = Math.Min(argCount, argRegs4.Length);

            // on windows we need the shadow space (4 * pointerSize), so we dont deallocate before function call
            if (targetOS != OSPlatform.Windows)
            {
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
        }

        public void PostCallDeallocate(int allocated, int argCount)
        {
            string[] argRegs4 = targetOS == OSPlatform.Linux ? argRegister4B : argRegisterWin4B;
            //string[] argRegs = targetOS == OSPlatform.Linux ? argRegister8B : argRegisterWin8B;
            int regsUsed = Math.Min(argCount, argRegs4.Length);

            if (targetOS != OSPlatform.Windows)
            {
                if (argCount > regsUsed)
                {
                    // post deallocate temp memory, we dont ned args on memory anymore
                    DeallocateMemory(allocated - regsUsed * pointerSize);
                }
            }
            else
            {
                // on windows we need to deallocate the shadow space as well, where we moved args but didnt pop them
                DeallocateMemory(allocated);
            }
        }

        public void CallFunction(string name)
        {
            Instruction("call " + name);
        }

        public void PushLeftOperand()
        {
            Instruction("pushq %rax");
            pushCounter++;
        }

        public void PopLeftOperand()
        {
            Instruction("movl %eax, %ecx"); // need to switch src and dest for - and /
            Instruction("popq %rax");
            pushCounter--;
        }

        public void CompareZero()
        {
            Instruction("cmpl $0, %eax");
        }

        public void SetIfNotEqual()
        {
            IntegerConstant(0); // zero out eax
            Instruction("setne %al");
        }

        public void Jump(string label)
        {
            Instruction("jmp " + label);
        }

        public void JumpEqual(string label)
        {
            Instruction("je " + label);
        }

        public void JumpNotEqual(string label)
        {
            Instruction("jne " + label);
        }

        public void IntegerConstant(int value)
        {
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

        public void UnaryOperation(char op)
        {
            switch (op)
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
