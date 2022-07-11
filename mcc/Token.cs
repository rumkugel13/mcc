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

    class Symbol2 : Token
    {
        public string Value;

        public Symbol2(string symbol)
        {
            Type = TokenType.SYMBOL2;
            Value = symbol;
        }

        public static HashSet<string> Dual = new HashSet<string>
        {
            "&&",
            "||",
            "==",
            "!=",
            "<=",
            ">=",
            "<<",
            ">>",
        };

        public static HashSet<string> Comparison = new HashSet<string>
        {
            "==",
            "!=",
            "<=",
            ">=",
            "<",
            ">",
        };

        public static HashSet<string> ShortCircuit = new HashSet<string>
        {
            "&&",
            "||",
        };

        public override string ToString()
        {
            return base.ToString() + " " + Value;
        }
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
            '%',
            '^',
            '?',
            ':',
            ',',
        };

        public static HashSet<char> Unary = new HashSet<char>
        {
            '+',
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
            '<',
            '>',
            '%',
            '&',
            '|',
            '^',
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

    abstract class Token
    {
        public TokenType Type;
        public int Line, Column;

        public enum TokenType
        {
            KEYWORD,
            SYMBOL,
            SYMBOL2,
            IDENTIFIER,
            INTEGER,
            END
        }

        public override string ToString()
        {
            return "Line: " + Line + ", Column: " + Column + " | " + Type.ToString();
        }
    }
}