namespace mcc.Backends
{
    internal interface IBackend
    {
        void GenerateGlobalVariable(string name, int val);
        void GenerateGlobalVariableAddress(string name);
        void GenerateUninitializedGlobalVariable(string name);

        void FunctionPrologue(string name);
        void FunctionEpilogue();

        void StoreGlobalVariable(string name);
        void LoadGlobalVariable(string name);
        void StoreLocalVariable(int byteOffset);
        void LoadLocalVariable(int byteOffset);
        void InitializeLocalVariable(int byteOffset);

        void StoreInt(int offset);
        void AllocateMemory(int bytesToAllocate);
        void DeallocateMemory(int bytesToDeallocate);

        void MoveRegisterToMemory(string register, int offset);
        void MoveMemoryToRegister(string register, int offset);

        public static int Align(int bytes, int align) => align * ((bytes + align - 1) / align);
        int AllocateAtLeast(int bytes);
        void MoveArgsIntoRegisters(int argCount);
        void MoveRegistersIntoMemory(int argCount);
        void PreCallDeallocate(int allocated, int argCount);
        void PostCallDeallocate(int allocated, int argCount);
        void CallFunction(string name);

        void PushLeftOperand();
        void PopLeftOperand();

        void CompareZero();
        void SetIfNotEqual();

        void Jump(string label);
        void JumpEqual(string label);
        void JumpNotEqual(string label);

        void IntegerConstant(int value);

        void ComparisonOperation(string op);
        void BinaryOperation(string op);
        void UnaryOperation(char op);

        void Label(string label);

        void Instruction(string instruction);

        string GetAssembly();
    }
}