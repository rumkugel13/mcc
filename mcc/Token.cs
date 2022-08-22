namespace mcc
{
    class Keyword : Token
    {
        public string Value = "";
        public KeywordTypes KeywordType;

        public Keyword(string keyword)
        {
            Type = TokenType.KEYWORD;
            Value = keyword;
            KeywordType = Keywords[keyword];
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
            return base.ToString() + " " + Value;
        }
    }

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

    class Symbol : Token
    {
        public string Value;

        public Symbol(string symbol)
        {
            Type = TokenType.SYMBOL;
            Value = symbol;
        }

        public static HashSet<string> Symbols = new HashSet<string>
        {
            "{",
            "}",
            "(",
            ")",
            ";",
            "-",
            "~",
            "!",
            "+",
            "*",
            "/",
            "&",
            "|",
            "=",
            "<",
            ">",
            "%",
            "^",
            "?",
            ":",
            ",",
            "&&",
            "||",
            "==",
            "!=",
            "<=",
            ">=",
            "<<",
            ">>",
        };

        public static HashSet<string> Unary = new HashSet<string>
        {
            "+",
            "-",
            "~",
            "!",
        };

        public static HashSet<string> Binary = new HashSet<string>
        {
            "*",
            "/",
            "+",
            "-",
            "<",
            ">",
            "%",
            "&",
            "|",
            "^",
            "&&",
            "||",
            "==",
            "!=",
            "<=",
            ">=",
            "<<",
            ">>",
        };

        public override string ToString()
        {
            return base.ToString() + " " + Value;
        }
    }

    class EndToken : Token
    {
        public EndToken()
        {
            Type = TokenType.END;
        }
    }

    class UnknownToken : Token
    {
        public UnknownToken()
        {
            Type = TokenType.UNKNOWN;
        }
    }

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