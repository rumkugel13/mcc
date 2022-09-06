namespace mcc
{
    abstract class Token
    {
        public TokenType Type;
        public int Line, Column;

        public enum TokenType
        {
            KEYWORD,
            SYMBOL,
            IDENTIFIER,
            INTEGER,
            UNKNOWN,
            END
        }

        public override string ToString()
        {
            return "Line: " + Line + ", Column: " + Column + " | " + Type.ToString();
        }
    }
}