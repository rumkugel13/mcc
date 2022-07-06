using System.Text;

namespace mcc
{
    abstract class AST
    {
        public virtual void Parse(Parser parser) { }
        public virtual void Print(int indent) { }
        public virtual void GenerateX86(Generator generator) { }
    }
}