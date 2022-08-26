using System.Diagnostics;
using System.Text;

namespace mcc
{
    internal class Program
    {
        static bool silent = true;
        static bool debug = false;
        static string VersionString = "mcc v0.15";
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
                        PrintVersion();
                    }
                    break;
                case 2:
                    if (args[0].Equals("-t"))
                    {
                        silent = false;

                        string path = args[1];
                        if (path.EndsWith(".c"))
                        {
                            if (!File.Exists(path))
                            {
                                Console.WriteLine("Unknown file: " + path);
                                return;
                            }

                            TestOne(path);
                        }
                        else
                        {
                            if (!Directory.Exists(path))
                            {
                                Console.WriteLine("Unknown folder: " + path);
                                return;
                            }

                            TestAll(path);
                        }
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
                    else if (args[0].Equals("-i"))
                    {
                        silent = true;
                        debug = false;
                        string filePath = args[1];
                        Interpret(filePath, out int interpreted);
                        Console.WriteLine(interpreted);
                    }

                    break;
            }
        }

        static void PrintVersion()
        {
            Console.WriteLine(VersionString);
        }

        static void PrintUsage()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("Usage:");
            builder.AppendLine("   mcc [-p | -d] <c-file>");
            builder.AppendLine("   mcc -v");
            builder.AppendLine("   mcc -t [<stage_folder> | <c-file>]");
            builder.AppendLine("   mcc -i <c-file>");
            builder.AppendLine("Options:");
            builder.AppendLine("   <c-file>             Compile c-file silently");
            builder.AppendLine("   -p <c-file>          Compile c-file with Verbose output");
            builder.AppendLine("   -d <c-file>          Compile c-file with Debug output");
            builder.AppendLine("   -v                   Print Version");
            builder.AppendLine("   -t <stage_folder>    Test c-files in stage folder");
            builder.AppendLine("   -t <c-file>          Test single c-file");
            builder.AppendLine("   -i <c-file>          Interpret single c-file");
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
                validCount += TestOne(validFile) ? 1 : 0;
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

        static bool TestOne(string sourceFile)
        {
            bool valid = false;
            if (Compile(sourceFile))
            {
                if (!silent) Console.Write("Running gcc ... ");
                GccCompile(sourceFile);
                if (!silent) Console.WriteLine("OK");
                if (TryGetExitCode(sourceFile.Replace(".c", ".out"), out int expected))
                {
                    if (TryGetExitCode(sourceFile.Replace(".c", Exe), out int got))
                    {
                        Console.WriteLine($"Comparing Results ... Expected: {expected}, Got: {got} " + (expected == got ? "OK" : "Fail"));
                        Interpret(sourceFile, out int interpreted);
                        Console.WriteLine("Interpreted value = " + (interpreted < 0 ? (interpreted + 256) : interpreted));
                        if (expected == got)
                            valid = true;
                    }
                }
            }
            File.Delete(sourceFile.Replace(".c", Exe));
            File.Delete(sourceFile.Replace(".c", ".out"));
            return valid;
        }

        static bool TryGetExitCode(string file, out int exitCode)
        {
            exitCode = 0;
            using (Process? process = Process.Start(file))
            {
                if (process.WaitForExit(Convert.ToInt32(TimeSpan.FromSeconds(10).TotalMilliseconds)))
                {
                    exitCode = process.ExitCode;
                }
                else
                {
                    exitCode = -1;
                    Console.WriteLine($"Fail: Process {file} failed to finish.");
                    process.Kill();
                    process.WaitForExit();
                    return false;
                }
            };

            return true;
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
            string command = $"{filePath} -o {filePath.Replace(sourceFile ? ".c" : ".s", sourceFile ? ".out" : Exe)}";
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
            catch (Exception exception)
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

        static void PrintTokenList(IReadOnlyList<Token> tokens)
        {
            foreach (Token token in tokens)
            {
                Console.WriteLine(token.ToString());      
            }
        }

        static IReadOnlyList<Token> Lex(string file)
        {
            string content = string.Join("\n", File.ReadAllLines(file)).TrimEnd();
            Lexer lexer = new Lexer(content);
            return lexer.GetAllTokens();
        }

        static ASTProgramNode ParseProgramNode(string programName, IReadOnlyList<Token> tokens)
        {
            Parser parser = new Parser(tokens, programName);
            return parser.ParseProgram();
        }

        static string GenerateFromASTNode(ASTNode program)
        {
            if (System.Runtime.InteropServices.RuntimeInformation.OSArchitecture == System.Runtime.InteropServices.Architecture.Arm64)
            {
                ArmGenerator generator = new ArmGenerator(program);
                return generator.GenerateARM();
            }
            else if (System.Runtime.InteropServices.RuntimeInformation.OSArchitecture == System.Runtime.InteropServices.Architecture.X64)
            {
                Generator generator = new Generator(program);
                return generator.GenerateX86();
            }
            else
            {
                return "";
            }
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

        static bool VerifyAndGenerateAST(string filePath, out ASTProgramNode program)
        {
            program = new ASTProgramNode("", new List<ASTTopLevelItemNode>());
            if (!File.Exists(filePath))
            {
                Console.WriteLine("Unknown file: " + filePath);
                return false;
            }

            bool finished = true;
            if (!silent) Console.WriteLine("Input: " + filePath);
            if (!silent && debug) Console.WriteLine(File.ReadAllText(filePath));

            try
            {
                //lexer
                var tokens = Lex(filePath);

                //nodeparser
                program = ParseProgramNode(Path.GetFileNameWithoutExtension(filePath), tokens);

                //validator
                ValidateASTNode(program);
            }
            catch (Exception exception)
            {
                if (!silent) Console.WriteLine(exception.Message);
                finished = false;
            }

            return finished;
        }

        static int Interpret(ASTProgramNode program)
        {
            Interpreter interpreter = new Interpreter(program);
            return interpreter.Interpret();
        }

        static bool Interpret(string filePath, out int returnValue)
        {
            returnValue = 0;

            if (VerifyAndGenerateAST(filePath, out ASTProgramNode program))
            {
                returnValue = Interpret(program);
                return true;
            }
            return false;
        }
    }
}