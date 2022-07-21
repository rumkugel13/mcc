
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

        public bool HasMoreTokens()
        {
            return streamIndex < stream.Length;
        }

        public Token GetNextToken()
        {
            if (!HasMoreTokens())
            {
                return new EndToken();
            }

            SkipWhiteSpace();

            char currentChar = stream[streamIndex];

            int line = currentLine;
            int column = currentColumn;

            if (Symbol.Symbols.Contains(currentChar))
            {
                // symbol
                streamIndex++;
                currentColumn++;

                if (streamIndex < stream.Length && Symbol.Symbols.Contains(stream[streamIndex]))
                {
                    // dual symbol
                    string temp = stream.Substring(streamIndex - 1, 2);

                    if (Symbol2.Dual.Contains(temp))
                    {
                        streamIndex++;
                        currentColumn++;
                        return new Symbol2(temp) { Line = line, Column = column };
                    }
                    else
                    {
                        return new Symbol(currentChar) { Line = line, Column = column };
                    }
                }
                else
                {
                    return new Symbol(currentChar) { Line = line, Column = column };
                }
            }
            else if (char.IsDigit(currentChar))
            {
                // integer
                int start = streamIndex;

                do
                {
                    streamIndex++;
                    currentColumn++;
                }
                while (streamIndex < stream.Length && char.IsDigit(stream[streamIndex]));

                int temp = int.Parse(stream.Substring(start, streamIndex - start));

                return new Integer(temp) { Line = line, Column = column };
            }
            else if (char.IsLetterOrDigit(currentChar))
            {
                // keyword or identifier
                int start = streamIndex;

                do
                {
                    streamIndex++;
                    currentColumn++;
                }
                while (streamIndex < stream.Length && (char.IsLetterOrDigit(stream[streamIndex]) || stream[streamIndex].Equals('_')));

                string temp = stream.Substring(start, streamIndex - start);

                if (Keyword.Keywords.ContainsKey(temp))
                    return new Keyword(temp) { Line = line, Column = column };
                else
                    return new Identifier(temp) { Line = line, Column = column };
            }
            else
            {
                throw new UnknownTokenException("Fail: Unkown Character or Symbol: " + currentChar);
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
                    streamIndex++;
                    currentColumn++;
                }
                else
                    break;
            }
        }
    }
}