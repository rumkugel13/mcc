using System.Diagnostics;
using System.Text;

namespace mcc
{
    internal class Program
    {
        static bool silent = true;
        static bool debug = false;
        static string VersionString = "mcc v0.12";
        static string Exe = ".exe";
        static char Sep = ';';

        static void Main(string[] args)
        {
            if (OperatingSystem.IsLinux())
            {
                Exe = "";
                Sep = ':';
            }

            switch (args.Length)
            {
                case 0:
                    PrintUsage();
                    return;
                case 1:
                    if (!args[0].StartsWith('-'))
                    {
                        string filePath = args[0];
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
                        Compile(filePath);
                    }
                    else if (args[0].Equals("-d"))
                    {
                        silent = false;
                        debug = true;
                        string filePath = args[1];
                        Compile(filePath);
                    }

                    break;
            }
        }

        static void PrintUsage()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("Usage:");
            builder.AppendLine("   mcc [-p | -d] <c-file>");
            builder.AppendLine("   mcc -v");
            builder.AppendLine("   mcc -t <stage_folder>");
            builder.AppendLine("Options:");
            builder.AppendLine("   <c-file>             Compile c-file silently");
            builder.AppendLine("   -p <c-file>          Compile c-file with Verbose output");
            builder.AppendLine("   -d <c-file>          Compile c-file with Debug output");
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
                {
                    if (!silent) Console.Write("Running gcc ... ");
                    GccCompile(validFile);
                    if (!silent) Console.WriteLine("OK");
                    int expected = GetExitCode(validFile.Replace(".c", ".out"));
                    int got = GetExitCode(validFile.Replace(".c", Exe));
                    Console.WriteLine($"Comparing Results ... Expected: {expected}, Got: {got} " + (expected == got ? "OK" : "Fail"));
                    if (expected == got)
                        validCount++;
                }
                File.Delete(validFile.Replace(".c", Exe));
                File.Delete(validFile.Replace(".c", ".out"));
            }

            int invalidCount = 0;
            foreach (string invalidFile in invalidFiles)
            {
                if (!Compile(invalidFile))
                    invalidCount++;
                File.Delete(invalidFile.Replace(".c", Exe));
            }

            Console.WriteLine($"Valid: {validCount}/{validFiles.Length}, Invalid: {invalidCount}/{invalidFiles.Length}");
        }

        static int GetExitCode(string file)
        {
            int exitCode = 0;
            using (Process? process = Process.Start(file))
            {
                process.WaitForExit();

                exitCode = process.ExitCode;
            };

            return exitCode;
        }

        static bool GccCompile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                Console.WriteLine("Unknown file: " + filePath);
                return false;
            }

            string gccPath = GetFullPath("gcc" + Exe);
            if (string.IsNullOrEmpty(gccPath))
            {
                Console.WriteLine("Fail: Couldn't find gcc");
                return false;
            }

            bool sourceFile = filePath.EndsWith(".c");
            bool success = false;
            string command = $"-m64 {filePath} -o {filePath.Replace(sourceFile ? ".c" : ".s", sourceFile ? ".out" : Exe)}";
            using (Process? process = Process.Start(gccPath, command))
            {
                process.WaitForExit();

                if (process.ExitCode == 0)
                {
                    success = true;
                }
            };

            return success;
        }

        static bool Compile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                Console.WriteLine("Unknown file: " + filePath);
                return false;
            }

            bool finished = true;
            string assemblyFile = filePath.Replace(".c", ".s");
            if (!silent) Console.WriteLine("Input: " + filePath);
            if (!silent && debug) Console.WriteLine(File.ReadAllText(filePath));

            try
            {
                //lexer
                if (!silent) Console.Write("Running Lexer ... ");
                var tokens = Lex(filePath);
                if (!silent) Console.WriteLine("OK");
                if (!silent && debug) PrintTokenList(tokens);

                //nodeparser
                if (!silent) Console.Write("Parsing Tokens ... ");
                ASTProgramNode program = ParseProgramNode(Path.GetFileNameWithoutExtension(filePath), tokens);
                if (!silent) Console.WriteLine("OK");
                if (!silent && debug) PrintFromASTNode(program);

                //validator
                if (!silent) Console.Write("Validating AST ... ");
                ValidateASTNode(program);
                if (!silent) Console.WriteLine("OK");

                //nodegenerator
                if (!silent) Console.Write("Generating Assembly ... ");
                string assembly = GenerateFromASTNode(program);
                if (!silent) Console.WriteLine("OK");
                if (!silent && debug) PrintAssembly(assembly);

                //writer
                if (!silent) Console.Write("Writing Assembly File ... ");
                File.WriteAllText(assemblyFile, assembly);
                if (!silent) Console.WriteLine("OK");

                //assembler
                if (!silent) Console.Write("Running Assembler ... ");
                bool success = GccCompile(assemblyFile);
                if (!silent) Console.WriteLine(success ? "OK" : "FAIL");
                if (!success) finished = false;
            }
            catch (NotImplementedException exception)
            {
                if (!silent) Console.WriteLine(exception.Message);
                finished = false;
            }
            catch (UnknownTokenException exception)
            {
                if (!silent) Console.WriteLine(exception.Message);
                finished = false;
            }
            catch (UnexpectedValueException exception)
            {
                if (!silent) Console.WriteLine(exception.Message);
                finished = false;
            }
            catch (ASTVariableException exception)
            {
                if (!silent) Console.WriteLine(exception.Message);
                finished = false;
            }
            catch (ASTLoopScopeException exception)
            {
                if (!silent) Console.WriteLine(exception.Message);
                finished = false;
            }
            catch (ASTFunctionException exception)
            {
                if (!silent) Console.WriteLine(exception.Message);
                finished = false;
            }
            finally
            {
                if (File.Exists(assemblyFile))
                {
                    //remove assembly file
                    if (!silent) Console.Write("Cleaning Up ... ");
                    File.Delete(assemblyFile);
                    if (!silent) Console.WriteLine("OK");
                }
            }

            return finished;
        }

        static string GetFullPath(string executable)
        {
            string? temp = Environment.GetEnvironmentVariable("PATH");
            if (!string.IsNullOrEmpty(temp))
            {
                foreach (string folder in temp.Split(Sep))
                {
                    string result = Path.Combine(folder, executable);
                    if (File.Exists(result))
                        return result;
                }
            }

            return "";
        }

        static void PrintAssembly(string assembly)
        {
            Console.WriteLine(assembly);
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

            string content = string.Join("\n", File.ReadAllLines(file)).TrimEnd();
            Lexer lexer = new Lexer(content);

            while (lexer.HasMoreTokens())
            {
                tokens.Add(lexer.GetNextToken());
            }

            return tokens;
        }

        static ASTProgramNode ParseProgramNode(string programName, List<Token> tokens)
        {
            Parser parser = new Parser(tokens, programName);
            return parser.ParseProgram();
        }

        static string GenerateFromASTNode(ASTNode program)
        {
            return new Generator(program).GenerateX86();
        }

        static void PrintFromASTNode(ASTNode program)
        {
            PrettyPrinter printer = new PrettyPrinter(program);
            Console.WriteLine(printer.Print());
        }

        static void ValidateASTNode(ASTNode program)
        {
            Validator validator = new Validator(program);
            validator.ValidateX86();
        }
    }
}