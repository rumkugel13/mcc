using mcc.Backends;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace mcc
{
    internal class CompilationEngine
    {
        public string FileExtension = ".exe";
        public bool Silent = true;
        public bool Debug = false;

        public Architecture TargetArch = Architecture.X64;
        public OSPlatform TargetOS = OSPlatform.Windows;

        public CompilationEngine() { }

        public void TestAll(string stage)
        {
            if (!Directory.Exists(stage))
            {
                Console.WriteLine("Unknown folder: " + stage);
                return;
            }

            Console.WriteLine("Testing all in " + stage);
            string validPath = Path.Combine(stage, "valid");
            string invalidPath = Path.Combine(stage, "invalid");

            string[] validFiles = Directory.Exists(validPath) ? Directory.GetFiles(validPath, "*.c", SearchOption.AllDirectories) : Array.Empty<string>();
            string[] invalidFiles = Directory.Exists(invalidPath) ? Directory.GetFiles(invalidPath, "*.c", SearchOption.AllDirectories) : Array.Empty<string>();

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
                File.Delete(invalidFile.Replace(".c", this.FileExtension));
            }

            Console.WriteLine($"Valid: {validCount}/{validFiles.Length}, Invalid: {invalidCount}/{invalidFiles.Length}");
        }

        public bool TestOne(string sourceFile)
        {
            if (!File.Exists(sourceFile))
            {
                Console.WriteLine("Unknown file: " + sourceFile);
                return false;
            }

            if (Compile(sourceFile))
            {
                if (!this.Silent) Console.Write("Compiling for reference ... ");
                GccCompile(sourceFile);
                if (!this.Silent) Console.WriteLine("OK");
                string outPath = sourceFile.Replace(".c", ".out");
                if (TryGetExitCode(outPath, out int expected))
                {
                    File.Delete(outPath);
                    string exePath = sourceFile.Replace(".c", this.FileExtension);
                    if (TryGetExitCode(exePath, out int got))
                    {
                        File.Delete(exePath);
                        Console.WriteLine($"Comparing Results ... Expected: {expected}, Got: {got} " + (expected == got ? "OK" : "Fail"));
                        Interpret(sourceFile, out int interpreted);
                        Console.WriteLine("Interpreted value = " + interpreted);
                        BytecodeInterpret(sourceFile, out int bcInt);
                        Console.WriteLine("Bytecode value = " + bcInt);
                        if (expected == got)
                            return true;
                    }
                }
            }
            return false;
        }

        public bool TryGetExitCode(string file, out int exitCode)
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

        public bool GccCompile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                Console.WriteLine("Unknown file: " + filePath);
                return false;
            }

            string gccPath = GetFullPath("gcc" + this.FileExtension);
            if (string.IsNullOrEmpty(gccPath))
            {
                Console.WriteLine("Fail: Couldn't find gcc");
                return false;
            }

            bool sourceFile = filePath.EndsWith(".c");
            bool success = false;
            string command = $"{filePath} -o {filePath.Replace(sourceFile ? ".c" : ".s", sourceFile ? ".out" : this.FileExtension)}";
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

        public bool Compile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                Console.WriteLine("Unknown file: " + filePath);
                return false;
            }

            bool finished = true;
            string assemblyFile = filePath.Replace(".c", ".s");
            if (!this.Silent) Console.WriteLine("Input: " + filePath);
            if (!this.Silent && this.Debug) Console.WriteLine(File.ReadAllText(filePath));

            try
            {
                //lexer
                if (!this.Silent) Console.Write("Running Lexer ... ");
                var tokens = Lex(filePath);
                if (!this.Silent) Console.WriteLine("OK");
                if (!this.Silent && this.Debug) PrintTokenList(tokens);

                //nodeparser
                if (!this.Silent) Console.Write("Parsing Tokens ... ");
                ASTProgramNode program = ParseProgramNode(Path.GetFileNameWithoutExtension(filePath), tokens);
                if (!this.Silent) Console.WriteLine("OK");
                if (!this.Silent && this.Debug) PrintFromASTNode(program);

                //validator
                if (!this.Silent) Console.Write("Validating AST ... ");
                ValidateASTNode(program);
                if (!this.Silent) Console.WriteLine("OK");

                //nodegenerator
                if (!this.Silent) Console.Write("Generating Assembly ... ");
                string assembly = GenerateAssemblyFromASTNode(program);
                if (!this.Silent) Console.WriteLine("OK");
                if (!this.Silent && this.Debug) PrintAssembly(assembly);

                //writer
                if (!this.Silent) Console.Write("Writing Assembly File ... ");
                File.WriteAllText(assemblyFile, assembly);
                if (!this.Silent) Console.WriteLine("OK");

                //assembler
                if (!this.Silent) Console.Write("Running Assembler ... ");
                bool success = GccCompile(assemblyFile);
                if (!this.Silent) Console.WriteLine(success ? "OK" : "FAIL");
                if (!success) finished = false;
            }
            catch (Exception exception)
            {
                if (!this.Silent) Console.WriteLine(exception.Message);
                finished = false;
            }
            finally
            {
                if (File.Exists(assemblyFile))
                {
                    //remove assembly file
                    if (!this.Silent) Console.Write("Cleaning Up ... ");
                    File.Delete(assemblyFile);
                    if (!this.Silent) Console.WriteLine("OK");
                }
            }

            return finished;
        }

        public string GetFullPath(string executable)
        {
            char pathSeperator = OperatingSystem.IsWindows() ? ';' : ':';

            string? temp = Environment.GetEnvironmentVariable("PATH");
            if (!string.IsNullOrEmpty(temp))
            {
                foreach (string folder in temp.Split(pathSeperator))
                {
                    string result = Path.Combine(folder, executable);
                    if (File.Exists(result))
                        return result;
                }
            }

            return "";
        }

        public void PrintAssembly(string assembly)
        {
            Console.WriteLine(assembly);
        }

        public void PrintTokenList(IReadOnlyList<Token> tokens)
        {
            foreach (Token token in tokens)
            {
                Console.WriteLine(token.ToString());
            }
        }

        public IReadOnlyList<Token> Lex(string file)
        {
            string content = string.Join("\n", File.ReadAllLines(file));
            Lexer lexer = new Lexer(content);
            return lexer.GetAllTokens();
        }

        public ASTProgramNode ParseProgramNode(string programName, IReadOnlyList<Token> tokens)
        {
            Parser parser = new Parser(tokens, programName);
            return parser.ParseProgram();
        }

        public string GenerateAssemblyFromASTNode(ASTNode program)
        {
            IBackend backend;
            switch (this.TargetArch)
            {
                case Architecture.X64:
                    backend = new X64Backend(this.TargetOS); break;
                case Architecture.Arm64:
                    backend = new Arm64Backend(this.TargetOS); break;
                default: throw new NotSupportedException("Target Architecture " + this.TargetArch + " not supported.");
            }

            AsmGenerator generator = new AsmGenerator(program, backend);
            return generator.Generate();
        }

        public void PrintFromASTNode(ASTNode program)
        {
            PrettyPrinter printer = new PrettyPrinter(program);
            Console.WriteLine(printer.Print());
        }

        public void ValidateASTNode(ASTNode program)
        {
            Validator validator = new Validator(program);
            validator.ValidateAST();
        }

        public bool VerifyAndGenerateAST(string filePath, out ASTProgramNode program)
        {
            program = new ASTProgramNode("", new List<ASTTopLevelItemNode>());
            if (!File.Exists(filePath))
            {
                Console.WriteLine("Unknown file: " + filePath);
                return false;
            }

            bool finished = true;
            if (!this.Silent) Console.WriteLine("Input: " + filePath);
            if (!this.Silent && this.Debug) Console.WriteLine(File.ReadAllText(filePath));

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
                if (!this.Silent) Console.WriteLine(exception.Message);
                finished = false;
            }

            return finished;
        }

        public int Interpret(ASTProgramNode program)
        {
            Interpreter interpreter = new Interpreter(program);
            return interpreter.Interpret();
        }

        public bool Interpret(string filePath, out int returnValue)
        {
            returnValue = 0;

            if (VerifyAndGenerateAST(filePath, out ASTProgramNode program))
            {
                returnValue = Interpret(program);
                return true;
            }
            return false;
        }

        public int BytecodeInterpret(ASTProgramNode program, bool debug = false)
        {
            IBackend backend;
            backend = new BytecodeBackend();
            AsmGenerator generator = new AsmGenerator(program, backend);
            string assembly = generator.Generate();
            if (debug)
                Console.WriteLine(assembly);

            string filePath = "../../../bcvm/bcvm.exe";
            if (!File.Exists(filePath))
            {
                Console.WriteLine($"Fail: Couldn't find {filePath}.");
                return -9999;
            }
            string command = $"-i \"" + assembly + "\"";
            int exitCode = 0;
            using (Process? process = Process.Start(filePath, command))
            {
                if (process.WaitForExit(Convert.ToInt32(TimeSpan.FromSeconds(1).TotalMilliseconds)))
                {
                    exitCode = process.ExitCode;
                }
                else
                {
                    exitCode = -9999;
                    Console.WriteLine($"Fail: Process {filePath} failed to finish.");
                    process.Kill();
                    process.WaitForExit();
                }
            };

            return exitCode;
        }

        public bool BytecodeInterpret(string filePath, out int returnValue)
        {
            returnValue = 0;

            if (VerifyAndGenerateAST(filePath, out ASTProgramNode program))
            {
                returnValue = BytecodeInterpret(program);
                return true;
            }
            return false;
        }
    }
}
