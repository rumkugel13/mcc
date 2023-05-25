namespace mcc.Test
{
    [TestClass]
    public class LexerTest
    {
        private const string stringReturn0 = "int main() {\r\n    return 0;\r\n}";
        private readonly IReadOnlyList<Token> tokensReturn0 = new List<Token>() {
            new Keyword(Keyword.KeywordTypes.INT) { Position = new Token.TokenPos { Line = 1, Column = 1 } },
            new Identifier("main") { Position = new Token.TokenPos { Line = 1, Column = 5 } },
            new Symbol("(") { Position = new Token.TokenPos { Line = 1, Column = 9 } },
            new Symbol(")") { Position = new Token.TokenPos { Line = 1, Column = 10 } },
            new Symbol("{") { Position = new Token.TokenPos { Line = 1, Column = 12 } },
            new Keyword(Keyword.KeywordTypes.RETURN) { Position = new Token.TokenPos { Line = 2, Column = 5 } },
            new Integer(0) { Position = new Token.TokenPos { Line = 2, Column = 12 } },
            new Symbol(";") { Position = new Token.TokenPos { Line = 2, Column = 13 } },
            new Symbol("}") { Position = new Token.TokenPos { Line = 3, Column = 1 } },
        };

        [TestMethod]
        public void TestReturn0()
        {
            Lexer lexer = new Lexer(stringReturn0);
            var tokens = lexer.GetAllTokens();

            Assert.AreEqual(tokens.Count, tokensReturn0.Count);
            for (int i = 0; i < tokens.Count; i++)
            {
                Assert.AreEqual(tokensReturn0[i].ToString(), tokens[i].ToString());
            }
        }
    }
}