using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace mcc
{
    internal class Program
    {
        static bool silent = true;
        static bool debug = false;
        static string VersionString = "mcc v0.8";

        static void Main(string[] args)
        {
            switch (args.Length)
            {
                case 0:
                    PrintUsage();
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
                    else if (args[0].Equals("-v"))
                    {
                        Console.WriteLine(VersionString);
                    }
                    break;
                case 2:
                    if (args[0].Equals("-t"))
                    {
                        silent = false;
                        string stage = args[1];
                        if (!Directory.Exists(stage))
                        {
                            Console.WriteLine("Unknown folder.");
                            return;
                        }

                        TestAll(stage);
                    }
                    else if (args[0].Equals("-p"))
                    {
                        silent = false;
                        string filePath = args[1];

                        if (!File.Exists(filePath))
                        {
                            Console.WriteLine("Unknown file.");
                            return;
                        }

                        Compile(filePath);
                    }
                    else if (args[0].Equals("-d"))
                    {
                        silent = false;
                        debug = true;
                        string filePath = args[1];

                        if (!File.Exists(filePath))
                        {
                            Console.WriteLine("Unknown file.");
                            return;
                        }

                        Compile(filePath);
                    }

                    break;
            }
        }

        static void PrintUsage()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("Usage:");
            builder.AppendLine("   mcc [-p | -d] <c_file>");
            builder.AppendLine("   mcc -v");
            builder.AppendLine("   mcc -t <stage_folder>");
            builder.AppendLine("Options:");
            builder.AppendLine("   <c_file>             Compile c-file silently");
            builder.AppendLine("   -p <c_file>          Compile c-file with Verbose output");
            builder.AppendLine("   -d <c_file>          Compile c-file with Debug output");
            builder.AppendLine("   -v                   Print Version");
            builder.AppendLine("   -t <stage_folder>    Test c-files in stage folder");
            Console.Write(builder.ToString());
        }

        static void TestAll(string stage)
        {
            Console.WriteLine("Testing all in " + stage);
            string validPath = Path.Combine(stage, "valid");
            string invalidPath = Path.Combine(stage, "invalid");

            string[] validFiles = Directory.GetFiles(validPath, "*.c", SearchOption.AllDirectories);
            string[] invalidFiles = Directory.GetFiles(invalidPath, "*.c", SearchOption.AllDirectories);

            int validCount = 0;
            foreach (string validFile in validFiles)
            {
                if (Compile(validFile))
                    validCount++;
                File.Delete(validFile.Replace(".c", ".exe"));
            }

            int invalidCount = 0;
            foreach (string invalidFile in invalidFiles)
            {
                if (!Compile(invalidFile))
                    invalidCount++;
                File.Delete(invalidFile.Replace(".c", ".exe"));
            }

            Console.WriteLine($"Valid: {validCount}/{validFiles.Count()}, Invalid: {invalidCount}/{invalidFiles.Count()}");
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
                if (!silent && debug) PrintTokenList(tokens);

                //parser
                if (!silent) Console.Write("Parsing Tokens ... ");
                AST ast = Parse(tokens);
                if (!silent) Console.WriteLine("OK");
                if (!silent && debug) PrintAST(ast);

                //generator
                if (!silent) Console.Write("Generating Assembly ... ");
                string assembly = Generate(ast);
                if (!silent) Console.WriteLine("OK");
                if (!silent && debug) PrintAssembly(assembly);

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

                    if (process.ExitCode == 0)
                    {
                        if (!silent) Console.WriteLine("OK");
                    }
                    else
                    {
                        if (!silent) Console.WriteLine("FAIL");
                        finished = false;
                    }
                };
            }
            catch (UnknownTokenException exception)
            {
                Console.WriteLine(exception.Message);
                finished = false;
            }
            catch (UnexpectedValueException exception)
            {
                Console.WriteLine(exception.Message);
                finished = false;
            }
            catch (ASTVariableException exception)
            {
                Console.WriteLine(exception.Message);
                finished = false;
            }
            catch (ASTLoopScopeException exception)
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
            Generator generator = new Generator();
            ast.GenerateX86(generator);
            return generator.CreateOutput();
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
                Console.WriteLine(token.ToString());      
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
                tokens.Add(tokenizer.GetCurrentToken());
            }

            return tokens;
        }

        private static readonly Regex whitespace = new Regex(@"\s+");
    }
}