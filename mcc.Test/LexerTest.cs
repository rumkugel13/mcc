namespace mcc.Test
{
    [TestClass]
    public class LexerTest
    {
        private const string stringReturn0 = "int main() {\r\n    return 0;\r\n}";
        private readonly IReadOnlyList<Token> tokensReturn0 = new List<Token>() {
            new Keyword(Keyword.KeywordTypes.INT) { Line = 1, Column = 1 },
            new Identifier("main"){ Line = 1, Column = 5 },
            new Symbol("("){ Line = 1, Column = 9 },
            new Symbol(")"){ Line = 1, Column = 10 },
            new Symbol("{"){ Line = 1, Column = 12 },
            new Keyword(Keyword.KeywordTypes.RETURN){ Line = 2, Column = 5 },
            new Integer(0){ Line = 2, Column = 12 },
            new Symbol(";"){ Line = 2, Column = 13 },
            new Symbol("}"){ Line = 3, Column = 1 },
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