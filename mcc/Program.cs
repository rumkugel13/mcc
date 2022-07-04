using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace mcc
{
    internal class Program
    {
        static bool silent = false;

        static void Main(string[] args)
        {
            if (!silent) Console.WriteLine("mcc v0.3");

            switch (args.Length)
            {
                case 0:
                    Console.WriteLine("Missing parameter.");
                    return;
                case 1:
                    if (!args[0].StartsWith('-'))
                    {
                        string filePath = args[0];

                        if (!File.Exists(filePath))
                        {
                            Console.WriteLine("Unknown file.");
                            return;
                        }

                        Compile(filePath);
                    }
                    break;
                case 2:
                    if (args[0].Equals("-t"))
                    {
                        string stage = args[1];
                        if (!Directory.Exists(stage))
                        {
                            Console.WriteLine("Unknown folder.");
                            return;
                        }

                        Console.WriteLine("Testing all in " + stage);
                        string validPath = Path.Combine(stage, "valid");
                        string invalidPath = Path.Combine(stage, "invalid");

                        string[] validFiles = Directory.GetFiles(validPath).Where((a) => a.EndsWith(".c")).ToArray();
                        string[] invalidFiles = Directory.GetFiles(invalidPath).Where((a) => a.EndsWith(".c")).ToArray();

                        int validCount = 0;
                        foreach (string validFile in validFiles)
                        {
                            if (Compile(validFile))
                                validCount++;
                        }

                        int invalidCount = 0;
                        foreach (string invalidFile in invalidFiles)
                        {
                            if (!Compile(invalidFile))
                                invalidCount++;
                        }

                        Console.WriteLine($"Valid: {validCount}/{validFiles.Count()}, Invalid: {invalidCount}/{invalidFiles.Count()}");
                    }

                    break;
            }
        }

        static bool Compile(string filePath)
        {
            bool finished = true;
            string fileName = Path.GetFileNameWithoutExtension(filePath);
            string assemblyFile = Path.Combine(Path.GetDirectoryName(filePath), fileName + ".s");
            if (!silent) Console.WriteLine("Input: " + filePath);

            try
            {
                //lexer
                if (!silent) Console.Write("Running Lexer ... ");
                var tokens = Lex(filePath);
                if (!silent) Console.WriteLine("OK");
                //PrintTokenList(tokens);

                //parser
                if (!silent) Console.Write("Parsing Tokens ... ");
                AST ast = Parse(tokens);
                if (!silent) Console.WriteLine("OK");
                //PrintAST(ast);

                //generator
                if (!silent) Console.Write("Generating Assembly ... ");
                string assembly = Generate(ast);
                if (!silent) Console.WriteLine("OK");
                //PrintAssembly(assembly);

                //writer
                if (!silent) Console.Write("Writing Assembly File ... ");
                File.WriteAllText(assemblyFile, assembly);
                if (!silent) Console.WriteLine("OK");

                //assembler
                if (!silent) Console.Write("Running Assembler ... ");
                string path = GetFullPath("gcc.exe");
                if (string.IsNullOrEmpty(path))
                {
                    Console.WriteLine("Fail: Couldn't find gcc.exe");
                    return false;
                }

                string command = $"-m64 {Path.GetFullPath(assemblyFile)} -o {Path.Combine(Path.GetDirectoryName(Path.GetFullPath(assemblyFile)), fileName)}";
                ProcessStartInfo start = new ProcessStartInfo(path, command);
                //start.WorkingDirectory = Path.GetFullPath(path).Replace("gcc.exe", "");
                using (Process? process = System.Diagnostics.Process.Start(start))
                {
                    while (!process.HasExited) ;
                    if (!silent) Console.WriteLine(process.ExitCode == 0 ? "OK" : "FAIL");
                };
            }
            catch (InvalidDataException exception)
            {
                Console.WriteLine(exception.Message);
                finished = false;
            }
            catch (InvalidOperationException exception)
            {
                Console.WriteLine(exception.Message);
                finished = false;
            }
            finally
            {
                //remove assembly file
                if (!silent) Console.Write("Cleaning Up ... ");
                File.Delete(assemblyFile);
                if (!silent) Console.WriteLine("OK");
            }

            return finished;
        }

        static string GetFullPath(string executable)
        {
            string? temp = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Machine);
            if (!string.IsNullOrEmpty(temp))
            {
                foreach (string folder in temp.Split(';'))
                {
                    string result = Path.Combine(folder, executable);
                    if (File.Exists(result))
                        return result;
                }
            }

            return "";
        }

        static string Generate(AST ast)
        {
            StringBuilder sb = new StringBuilder();
            ast.GenerateX86(sb);
            return sb.ToString();
        }

        static void PrintAssembly(string assembly)
        {
            Console.WriteLine(assembly);
        }

        static void PrintAST(AST ast)
        {
            ast.Print(0);
        }

        static AST Parse(List<Token> tokens)
        {
            Parser parser = new Parser(tokens);

            ASTProgram program = new ASTProgram();

            program.Parse(parser);

            if (parser.Failed())
            {
                Console.WriteLine("Failed to Parse program.");
            }

            return program;
        }

        static void PrintTokenList(List<Token> tokens)
        {
            foreach (Token token in tokens)
            {
                switch (token)
                {
                    case Symbol symbol: Console.WriteLine(token.Type + " " + symbol.Value); break;
                    case Integer integer: Console.WriteLine(token.Type + " " + integer.Value); break;
                    case Identifier identifier: Console.WriteLine(token.Type + " " + identifier.Value); break;
                    case Keyword keyword: Console.WriteLine(token.Type + " " + keyword.Value); break;
                    default: Console.WriteLine(token.Type); break;
                }              
            }
        }

        static List<Token> Lex(string file)
        {
            List<Token> tokens = new();

            string[] content = File.ReadAllLines(file);

            // put all in one line
            StringBuilder complete = new StringBuilder();
            foreach (string line in content)
            {
                complete.AppendLine(line);
            }

            Tokenizer tokenizer = new Tokenizer(complete.ToString().Trim());

            while (tokenizer.HasMoreTokens())
            {
                tokenizer.Advance();

                string currentToken = tokenizer.CurrentToken();
                if (string.IsNullOrEmpty(currentToken))
                    break;

                switch (tokenizer.CurrentType())
                {
                    case Token.TokenType.KEYWORD:
                        tokens.Add(new Keyword(currentToken));
                        break;
                    case Token.TokenType.SYMBOL:
                        tokens.Add(new Symbol(currentToken[0]));
                        break;
                    case Token.TokenType.IDENTIFIER:
                        tokens.Add(new Identifier(currentToken));
                        break;
                    case Token.TokenType.INTEGER:
                        tokens.Add(new Integer(int.Parse(currentToken)));
                        break;
                }
            }

            return tokens;
        }

        private static readonly Regex whitespace = new Regex(@"\s+");
    }
}