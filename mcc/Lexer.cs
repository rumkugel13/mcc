
namespace mcc
{
    class Lexer
    {
        private string stream;
        private int streamIndex = 0, currentLine = 1, currentColumn = 1;

        public Lexer(string stream)
        {
            this.stream = stream;
        }

        public IReadOnlyList<Token> GetAllTokens()
        {
            List<Token> tokens = new();
            while (HasMoreTokens())
            {
                tokens.Add(GetNextToken());
            }
            return tokens;
        }

        private bool HasMoreTokens()
        {
            return streamIndex < stream.Length;
        }

        private Token GetNextToken()
        {
            if (!HasMoreTokens())
            {
                return new EndToken();
            }

            SkipWhiteSpace();

            char currentChar = stream[streamIndex];
            int line = currentLine;
            int column = currentColumn;
            int start = streamIndex;

            Advance();

            if (Symbol.Symbols.Contains(currentChar))
            {
                // symbol
                if (HasMoreTokens() && Symbol.Symbols.Contains(stream[streamIndex]) && Symbol2.Dual.Contains(stream.Substring(start, 2)))
                {
                    Advance();
                    return new Symbol2(stream.Substring(start, 2)) { Line = line, Column = column };
                }
                else
                    return new Symbol(currentChar) { Line = line, Column = column };
            }
            else if (char.IsDigit(currentChar))
            {
                // integer
                while (HasMoreTokens() && char.IsDigit(stream[streamIndex]))
                    Advance();

                int temp = int.Parse(stream.Substring(start, streamIndex - start));

                return new Integer(temp) { Line = line, Column = column };
            }
            else if (char.IsLetterOrDigit(currentChar))
            {
                // keyword or identifier
                while (HasMoreTokens() && (char.IsLetterOrDigit(stream[streamIndex]) || stream[streamIndex].Equals('_')))
                    Advance();

                string temp = stream.Substring(start, streamIndex - start);

                if (Keyword.Keywords.ContainsKey(temp))
                    return new Keyword(temp) { Line = line, Column = column };
                else
                    return new Identifier(temp) { Line = line, Column = column };
            }
            else
            {
                Fail("Fail: Unkown Character or Symbol: " + currentChar);
                return new UnknownToken();
            }
        }

        private void SkipWhiteSpace()
        {
            while (streamIndex < stream.Length - 1)
            {
                if (stream[streamIndex] == '\n')
                {
                    streamIndex++;
                    currentLine++;
                    currentColumn = 1;
                }
                else if (char.IsWhiteSpace(stream[streamIndex]))
                {
                    Advance();
                }
                else
                    break;
            }
        }

        private void Advance()
        {
            streamIndex++;
            currentColumn++;
        }

        private void Fail(string message)
        {
            Console.Error.WriteLine(message);
            throw new UnknownTokenException(message);
        }
    }
}