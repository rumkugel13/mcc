namespace mcc
{
    class Keyword : Token
    {
        public Keyword(string keyword)
        {
            Type = TokenType.KEYWORD;
            Value = keyword;
            KeywordType = Keywords[keyword];
        }

        public static Dictionary<string, KeywordTypes> Keywords = new Dictionary<string, KeywordTypes>
        {
            {"int", KeywordTypes.INT },
            {"return", KeywordTypes.RETURN },
        };

        public enum KeywordTypes
        {
            INT,
            RETURN,
        }
        public string Value = "";
        public KeywordTypes KeywordType;
    }

    class Identifier : Token
    {
        public Identifier(string name)
        {
            Type = TokenType.IDENTIFIER;
            Value = name;
        }

        public static string IdentifierRegEx = @"[a-zA-Z]\w*$";
        public string Value = "";
    }

    class Integer : Token
    {
        public Integer(int number)
        {
            Type = TokenType.INTEGER;
            Value = number;
        }

        public static string IntegerRegEx = "[0-9]+";
        public int Value;
    }

    class Symbol : Token
    {
        public char Value;

        public Symbol(char symbol)
        {
            Type = TokenType.SYMBOL;
            Value = symbol;
        }

        public static HashSet<char> Symbols = new HashSet<char>
        {
            '{',
            '}',
            '(',
            ')',
            ';',
            '-',
            '~',
            '!',
            '+',
            '*',
            '/',
            '&',
            '|',
            '=',
            '<',
            '>',
        };

        public static HashSet<char> Unary = new HashSet<char>
        {
            '-',
            '~',
            '!',
        };

        public static HashSet<char> Binary = new HashSet<char>
        {
            '*',
            '/',
            '+',
            '-',
        };
    }

    abstract class Token
    {
        public TokenType Type;

        public enum TokenType
        {
            KEYWORD,
            SYMBOL,
            SYMBOL2,
            IDENTIFIER,
            INTEGER,
        }
    }
}