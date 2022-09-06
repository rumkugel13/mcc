namespace mcc
{
    class Identifier : Token
    {
        public string Value = "";

        public Identifier(string name)
        {
            Type = TokenType.IDENTIFIER;
            Value = name;
        }

        public override string ToString()
        {
            return base.ToString() + " " + Value;
        }
    }
}