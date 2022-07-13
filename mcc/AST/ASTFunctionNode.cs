
namespace mcc
{
    class ASTFunctionNode : ASTNode
    {
        public string Name;
        public List<ASTStatementNode> Statements;
        public bool ContainsReturn;

        public ASTFunctionNode(string name, List<ASTStatementNode> statements)
        {
            Name = name;
            Statements = statements;
        }
    }
}