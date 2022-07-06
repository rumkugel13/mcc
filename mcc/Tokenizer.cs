using System.Text.RegularExpressions;

namespace mcc
{
    class Tokenizer
    {
        private string stream, current;
        private int streamIndex = 0;

        public Tokenizer(string stream)
        {
            this.stream = stream;
            this.current = "";
        }

        public void Advance()
        {
            if (!HasMoreTokens())
            {
                return;
            }

            char currentChar = stream[streamIndex];

            // skip empty space
            while (char.IsWhiteSpace(currentChar) && streamIndex < stream.Length - 1)
            {
                streamIndex++;
                currentChar = stream[streamIndex];
            }

            //if (!HasMoreTokens())
            //{
            //    return;
            //}

            if (Symbol.Symbols.Contains(currentChar))
            {
                // symbol
                streamIndex++;

                if (streamIndex < stream.Length && Symbol.Symbols.Contains(stream[streamIndex]))
                {
                    // dual symbol
                    string temp = stream.Substring(streamIndex - 1, 2);

                    if (Symbol2.Dual.Contains(temp))
                    {
                        streamIndex++;
                        current = temp;
                    }
                    else
                    {
                        current = currentChar.ToString();
                    }
                }
                else
                {
                    current = currentChar.ToString();
                }
            }
            else if (char.IsDigit(currentChar))
            {
                // integer
                int start = streamIndex;

                while (char.IsDigit(currentChar))
                {
                    streamIndex++;
                    currentChar = stream[streamIndex];
                }

                current = stream.Substring(start, streamIndex - start);
            }
            else if (char.IsLetterOrDigit(currentChar))
            {
                // keyword or identifier
                int start = streamIndex;
                string value = stream[streamIndex].ToString();

                while (Regex.Matches(value, Identifier.IdentifierRegEx).Count > 0)
                {
                    streamIndex++;
                    value += stream[streamIndex];
                }

                current = stream.Substring(start, streamIndex - start);
            }
            else
            {
                throw new UnknownTokenException("Fail: Unkown Character or Symbol: " + currentChar);
            }
        }

        public bool HasMoreTokens()
        {
            return streamIndex < stream.Length;
        }

        public Token GetCurrentToken()
        {
            if (string.IsNullOrEmpty(current))
                throw new UnknownTokenException("Fail: Unkown Token");

            switch (CurrentType())
            {
                case Token.TokenType.KEYWORD: return new Keyword(current);
                case Token.TokenType.SYMBOL: return new Symbol(current[0]);
                case Token.TokenType.SYMBOL2: return new Symbol2(current);
                case Token.TokenType.IDENTIFIER: return new Identifier(current);
                case Token.TokenType.INTEGER: return new Integer(int.Parse(current));
                default: throw new UnknownTokenException("Fail: Unkown Error");
            }
        }

        public Token.TokenType CurrentType()
        {
            if (Symbol2.Dual.Contains(current))
                return Token.TokenType.SYMBOL2;
            else if (Symbol.Symbols.Contains(current[0]))
                return Token.TokenType.SYMBOL;
            else if (Keyword.Keywords.ContainsKey(current))
                return Token.TokenType.KEYWORD;
            else if (int.TryParse(current, out int _))
                return Token.TokenType.INTEGER;
            else
                return Token.TokenType.IDENTIFIER;
        }
    }
}