namespace mcc
{
    class Keyword : Token
    {
        public KeywordTypes KeywordType;

        public Keyword(KeywordTypes keywordType)
        {
            Type = TokenType.KEYWORD;
            KeywordType = keywordType;
        }

        public static Dictionary<string, KeywordTypes> Keywords = new Dictionary<string, KeywordTypes>
        {
            { "int", KeywordTypes.INT },
            { "return", KeywordTypes.RETURN },
            { "if", KeywordTypes.IF },
            { "else", KeywordTypes.ELSE },
            { "for", KeywordTypes.FOR },
            { "while", KeywordTypes.WHILE },
            { "do", KeywordTypes.DO },
            { "break", KeywordTypes.BREAK },
            { "continue" , KeywordTypes.CONTINUE },
        };

        public enum KeywordTypes
        {
            INT,
            RETURN,
            IF,
            ELSE,
            FOR,
            WHILE,
            DO,
            BREAK,
            CONTINUE,
        }

        public override string ToString()
        {
            return base.ToString() + " " + KeywordType.ToString().ToLower();
        }
    }
}