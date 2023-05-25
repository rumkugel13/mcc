namespace mcc
{
    public abstract class Token
    {
        public TokenType Type;
        public TokenPos Position;

        public struct TokenPos
        {
            public int Line, Column;
            public override string ToString()
            {
                return "Pos: " + Line + ":" + Column;
            }
        }

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
            return Position.ToString() + " | " + Type.ToString();
        }
    }
}