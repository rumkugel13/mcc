namespace mcc
{
    class Integer : Token
    {
        public int Value;

        public Integer(int number)
        {
            Type = TokenType.INTEGER;
            Value = number;
        }

        public override string ToString()
        {
            return base.ToString() + " " + Value;
        }
    }
}