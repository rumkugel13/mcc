﻿namespace mcc
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
        SymbolTypes SymbolType;

        public Symbol(string symbol)
        {
            Type = TokenType.SYMBOL;
            Value = symbol;
            SymbolType = Symbols[symbol];
        }

        public enum SymbolTypes
        {
            OPEN_BRACKETS,
            CLOSE_BRACKETS,
            OPEN_PARENTHESIS,
            CLOSE_PARENTHESIS,
            SEMICOLON,
            MINUS,
            BIT_NEGATE,
            EXCLAMATION,
            PLUS,
            MULTIPLICATION,
            DIVISION,
            BIT_AND,
            BIT_OR,
            EQUALS,
            LESS_THAN,
            GREATER_THAN,
            REMAINDER,
            BIT_XOR,
            QUESTION,
            COLON,
            COMMA,
            LOGICAL_AND,
            LOGICAL_OR,
            DOUBLE_EQUALS,
            NOT_EQUALS,
            LESS_EQUAL,
            GREATER_EQUAL,
            SHIFT_LEFT,
            SHIFT_RIGHT,        
        }

        public static Dictionary<string, SymbolTypes> Symbols = new Dictionary<string, SymbolTypes>
        {
            { "{", SymbolTypes.OPEN_BRACKETS },
            { "}", SymbolTypes.CLOSE_BRACKETS },
            { "(", SymbolTypes.OPEN_PARENTHESIS },
            { ")", SymbolTypes.CLOSE_PARENTHESIS },
            { ";", SymbolTypes.SEMICOLON },
            { "-", SymbolTypes.MINUS },
            { "~", SymbolTypes.BIT_NEGATE },
            { "!", SymbolTypes.EXCLAMATION },
            { "+", SymbolTypes.PLUS },
            { "*", SymbolTypes.MULTIPLICATION },
            { "/", SymbolTypes.DIVISION },
            { "&", SymbolTypes.BIT_AND },
            { "|", SymbolTypes.BIT_OR },
            { "=", SymbolTypes.EQUALS },
            { "<", SymbolTypes.LESS_THAN },
            { ">", SymbolTypes.GREATER_THAN },
            { "%", SymbolTypes.REMAINDER },
            { "^", SymbolTypes.BIT_XOR },
            { "?", SymbolTypes.QUESTION },
            { ":", SymbolTypes.COLON },
            { ",", SymbolTypes.COMMA },
            { "&&", SymbolTypes.LOGICAL_AND },
            { "||", SymbolTypes.LOGICAL_OR },
            { "==", SymbolTypes.DOUBLE_EQUALS },
            { "!=", SymbolTypes.NOT_EQUALS },
            { "<=", SymbolTypes.LESS_EQUAL },
            { ">=", SymbolTypes.GREATER_EQUAL },
            { "<<", SymbolTypes.SHIFT_LEFT },
            { ">>", SymbolTypes.SHIFT_RIGHT },
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