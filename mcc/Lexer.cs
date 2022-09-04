
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

            bool skipped;
            do
            {
                SkipWhiteSpace();
                skipped = SkipComment();
                skipped |= SkipMultiLineComment();
            }
            while (skipped);

            char currentChar = stream[streamIndex];
            int line = currentLine;
            int column = currentColumn;
            int start = streamIndex;

            Advance();

            if (Symbol.Symbols.Contains(currentChar.ToString()))
            {
                // symbol
                if (HasMoreTokens() && Symbol.Symbols.Contains(stream[streamIndex].ToString()) && Symbol.Symbols.Contains(stream.Substring(start, 2)))
                {
                    Advance();
                    return new Symbol(stream.Substring(start, 2)) { Line = line, Column = column };
                }
                else
                    return new Symbol(currentChar.ToString()) { Line = line, Column = column };
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

                if (Keyword.Keywords.TryGetValue(temp, out var value))
                    return new Keyword(value) { Line = line, Column = column };
                else
                    return new Identifier(temp) { Line = line, Column = column };
            }
            else
            {
                Fail("Fail: Unkown Character or Symbol: " + currentChar);
                return new UnknownToken();
            }
        }

        private bool SkipMultiLineComment()
        {
            if (stream[streamIndex] == '/' && HasMoreTokens() && stream[streamIndex + 1] == '*')
            {
                // multiline comment
                while (HasMoreTokens())
                {
                    if (stream[streamIndex] == '\n')
                    {
                        streamIndex++;
                        currentLine++;
                        currentColumn = 1;
                    }
                    else if (stream[streamIndex] == '*' && HasMoreTokens())
                    {
                        Advance();
                        if (stream[streamIndex] == '/')
                        {
                            Advance();
                            SkipWhiteSpace();
                            break;
                        }
                    }
                    else
                        Advance();
                }

                if (!HasMoreTokens())
                {
                    Fail("Fail: Missing ending of multiline comment");
                }
                return true;
            }
            return false;
        }

        private bool SkipComment()
        {
            if (stream[streamIndex] == '/' && HasMoreTokens() && stream[streamIndex + 1] == '/')
            {
                // comment
                while (HasMoreTokens() && stream[streamIndex] != '\n')
                    Advance();
                SkipWhiteSpace();
                return true;
            }

            return false;
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
            //Console.Error.WriteLine(message);
            throw new UnknownTokenException(message);
        }
    }
}