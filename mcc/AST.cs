using System.Text;

namespace mcc
{
    abstract class AST
    {
        public static Dictionary<string, int> VariableMap = new Dictionary<string, int>();
        public static HashSet<string> FunctionReturn = new HashSet<string>();
        public static int StackIndex = -WordSize;
        public const int WordSize = 8;

        public virtual void Parse(Parser parser) { }
        public virtual void Print(int indent) { }
        public virtual void GenerateX86(StringBuilder stringBuilder) { }
    }
}