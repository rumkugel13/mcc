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
                current = currentChar.ToString();
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
                throw new InvalidDataException("Fail: Unkown Character or Symbol: " + currentChar);
            }
        }

        public bool HasMoreTokens()
        {
            return streamIndex < stream.Length;
        }

        public string CurrentToken()
        {
            return current;
        }

        public Token.TokenType CurrentType()
        {
            if (Symbol.Symbols.Contains(current[0]))
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