
namespace mcc
{
    public class Lexer
    {
        private string stream;
        private int streamIndex = 0, currentLine = 1, currentColumn = 1, startColumn = 1, tokenStart = 1;

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
                    SkipWhitespace();
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
            startColumn = currentColumn;
            tokenStart = streamIndex;

            Advance();

            if (Symbol.Symbols.ContainsKey(currentChar.ToString()))
            {
                return GetSymbol();
            }
            else if (char.IsDigit(currentChar))
            {
                return GetNumber();
            }
            else if (char.IsLetter(currentChar) || currentChar.Equals('_'))
            {
                return GetKeywordOrIdentifier();
            }
            else
            {
                Fail("Unkown Character or Symbol: " + currentChar);
                return new UnknownToken();
            }
        }

        private Token GetKeywordOrIdentifier()
                {
            while (HasMoreTokens() && (char.IsLetterOrDigit(stream[streamIndex]) || stream[streamIndex].Equals('_')))
                    Advance();

            string temp = stream.Substring(tokenStart, streamIndex - tokenStart);

            if (Keyword.Keywords.TryGetValue(temp, out var value))
                return new Keyword(value) { Position = GetTokenPos() };
                else
                return new Identifier(temp) { Position = GetTokenPos() };
            }

        private Token GetNumber()
        {
            char first = stream[tokenStart];
            if (first.Equals('0') && HasMoreTokens() && stream[streamIndex].Equals('x'))
            {
                return GetHexNumber();
            }
            else if (first.Equals('0') && HasMoreTokens() && stream[streamIndex].Equals('b'))
            {
                return GetBinaryNumber();
            }
            else
            {
                return GetDecimalNumber();
            }
        }

        private Token GetDecimalNumber()
        {
            while (HasMoreTokens() && char.IsDigit(stream[streamIndex]))
                Advance();

            int temp = int.Parse(stream.Substring(tokenStart, streamIndex - tokenStart));
            return new Integer(temp) { Position = GetTokenPos() };
        }

        private Token GetHexNumber()
                {
                    // hex number
                    Advance();  // skip 'x'
                    while (HasMoreTokens() && (char.IsDigit(stream[streamIndex]) 
                        || (stream[streamIndex] >= 'a' && stream[streamIndex] <= 'f') 
                        || (stream[streamIndex] >= 'A' && stream[streamIndex] <= 'F')))
                    {
                        Advance();
                    }

                string hexString = stream.Substring(tokenStart + 2, streamIndex - tokenStart - 2);
                    if (string.IsNullOrEmpty(hexString))
                    {
                    Fail("Invalid hex number at Line: " + currentLine + ", Column: " + startColumn);
                        return new UnknownToken();
                    }

                    int hexNum = Convert.ToInt32(hexString, 16);
            return new Integer(hexNum) {Position = GetTokenPos() };
                }

        private Token GetBinaryNumber()
                {
                    // binary number
                    Advance();  // skip 'b'
                    while (HasMoreTokens() && (stream[streamIndex].Equals('0') || stream[streamIndex].Equals('1')))
                    {
                        Advance();
                    }

                string binString = stream.Substring(tokenStart + 2, streamIndex - tokenStart - 2);
                    if (string.IsNullOrEmpty(binString))
                    {
                    Fail("Invalid binary number at Line: " + currentLine + ", Column: " + startColumn);
                        return new UnknownToken();
                    }

                    int binNum = Convert.ToInt32(binString, 2);
            return new Integer(binNum) {Position = GetTokenPos() };
                }

        private Symbol GetSymbol()
        {
            if (HasMoreTokens() && Symbol.Symbols.ContainsKey(stream[streamIndex].ToString()) && Symbol.Symbols.ContainsKey(stream.Substring(tokenStart, 2)))
            {
                // keyword or identifier
                while (HasMoreTokens() && (char.IsLetterOrDigit(stream[streamIndex]) || stream[streamIndex].Equals('_')))
                    Advance();
                return new Symbol(stream.Substring(tokenStart, 2)) {Position = GetTokenPos() };
            }
            else
                return new Symbol(stream[tokenStart].ToString()) {Position = GetTokenPos() };
        }

        private Token.TokenPos GetTokenPos()
        {
            return new Token.TokenPos { Line = currentLine, Column = startColumn };
        }

        private void SkipWhitespace()
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