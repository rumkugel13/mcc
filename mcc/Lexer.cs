
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
                if (char.IsWhiteSpace(stream[streamIndex]))
                {
                    if (stream[streamIndex] == '\n')
                    {
                        AdvanceLine();
                    }
                    else
                    {
                        Advance();
                    }
                }
                else if (stream[streamIndex].Equals('/') && (streamIndex + 1 < stream.Length) && (stream[streamIndex + 1].Equals('/') || stream[streamIndex + 1].Equals('*')))
                {
                    if (stream[streamIndex + 1].Equals('/'))
                    {
                        SkipComment();
                    }
                    else
                    {
                        SkipMultiLineComment();
                    }
                }
                else
                {
                    tokens.Add(GetNextToken());
                }
            }
            return tokens;
        }

        private bool HasMoreTokens()
        {
            return streamIndex < stream.Length;
        }

        private Token GetNextToken()
        {
            char currentChar = stream[streamIndex];
            int line = currentLine;
            int column = currentColumn;
            int start = streamIndex;

            Advance();

            if (Symbol.Symbols.ContainsKey(currentChar.ToString()))
            {
                // symbol
                if (HasMoreTokens() && Symbol.Symbols.ContainsKey(stream[streamIndex].ToString()) && Symbol.Symbols.ContainsKey(stream.Substring(start, 2)))
                {
                    Advance();
                    return new Symbol(stream.Substring(start, 2)) { Line = line, Column = column };
                }
                else
                    return new Symbol(currentChar.ToString()) { Line = line, Column = column };
            }
            else if (char.IsDigit(currentChar))
            {
                if (currentChar.Equals('0') && HasMoreTokens() && stream[streamIndex].Equals('x'))
                {
                    // hex number
                    Advance();  // skip 'x'
                    while (HasMoreTokens() && (char.IsDigit(stream[streamIndex]) 
                        || (stream[streamIndex] >= 'a' && stream[streamIndex] <= 'f') 
                        || (stream[streamIndex] >= 'A' && stream[streamIndex] <= 'F')))
                    {
                        Advance();
                    }

                    string hexString = stream.Substring(start + 2, streamIndex - start - 2);
                    if (string.IsNullOrEmpty(hexString))
                    {
                        Fail("Invalid hex number at Line: " + line + ", Column: " + column);
                        return new UnknownToken();
                    }

                    int hexNum = Convert.ToInt32(hexString, 16);
                    return new Integer(hexNum) { Line = line, Column = column };
                }
                else if (currentChar.Equals('0') && HasMoreTokens() && stream[streamIndex].Equals('b'))
                {
                    // binary number
                    Advance();  // skip 'b'
                    while (HasMoreTokens() && (stream[streamIndex].Equals('0') || stream[streamIndex].Equals('1')))
                    {
                        Advance();
                    }

                    string binString = stream.Substring(start + 2, streamIndex - start - 2);
                    if (string.IsNullOrEmpty(binString))
                    {
                        Fail("Invalid binary number at Line: " + line + ", Column: " + column);
                        return new UnknownToken();
                    }

                    int binNum = Convert.ToInt32(binString, 2);
                    return new Integer(binNum) { Line = line, Column = column };
                }

                // decimal integer
                while (HasMoreTokens() && char.IsDigit(stream[streamIndex]))
                    Advance();

                int temp = int.Parse(stream.Substring(start, streamIndex - start));

                return new Integer(temp) { Line = line, Column = column };
            }
            else if (char.IsLetter(currentChar) || currentChar.Equals('_'))
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
                Fail("Unkown Character or Symbol: " + currentChar);
                return new UnknownToken();
            }
        }

        private void SkipMultiLineComment()
        {
            // multiline comment
            while (HasMoreTokens())
            {
                if (stream[streamIndex] == '\n')
                {
                    AdvanceLine();
                }
                else if (stream[streamIndex] == '*' && HasMoreTokens())
                {
                    Advance();
                    if (stream[streamIndex] == '/')
                    {
                        // end of multiline comment
                        Advance();
                        break;
                    }
                }
                else
                {
                    Advance();
                }
            }

            if (!HasMoreTokens())
            {
                Fail("Missing ending of multiline comment");
            }
        }

        private void SkipComment()
        {
            // comment
            while (HasMoreTokens() && stream[streamIndex] != '\n')
            {
                Advance();
            }

            AdvanceLine();
        }

        private void AdvanceLine()
        {
            streamIndex++;
            currentLine++;
            currentColumn = 1;
        }

        private void Advance()
        {
            streamIndex++;
            currentColumn++;
        }

        private void Fail(string message)
        {
            //Console.Error.WriteLine("Fail: " + message);
            throw new UnknownTokenException("Fail: " + message);
        }
    }
}