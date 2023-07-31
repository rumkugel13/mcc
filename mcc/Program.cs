using System.Runtime.InteropServices;
using System.Text;

namespace mcc
{
    internal class Program
    {
        static readonly string VersionString = "mcc v0.16";

        static void Main(string[] args)
        {
            CompilationEngine engine = new CompilationEngine();

            if (OperatingSystem.IsLinux())
            {
                engine.FileExtension = "";
                engine.TargetOS = OSPlatform.Linux;
            }

            engine.TargetArch = RuntimeInformation.OSArchitecture;

            // todo: add -target option
            switch (args.Length)
            {
                case 0:
                    PrintUsage();
                    return;
                case 1:
                    if (!args[0].StartsWith('-'))
                    {
                        string filePath = args[0];
                        engine.Compile(filePath);
                    }
                    else if (args[0].Equals("-v") || args[0].Equals("--version"))
                    {
                        PrintVersion();
                    }
                    break;
                case 2:
                    string argument = args[0];
                    string value = args[1];
                    engine.Silent = false;

                    switch (argument)
                    {
                        case "-t":
                        case "--test":
                            if (value.EndsWith(".c"))
                            {
                                engine.TestOne(value);
                            }
                            else
                            {
                                engine.TestAll(value);
                            }
                            break;
                        case "-p":
                        case "--verbose":
                            engine.Compile(value);
                            break;
                        case "-d":
                        case "--debug":
                            engine.Debug = true;
                            engine.Compile(value);
                            break;
                        case "-i":
                        case "--interpret":
                            engine.Silent = true;
                            engine.Debug = false;
                            engine.Interpret(value, out int interpreted);
                            Console.WriteLine(interpreted);
                            break;
                        case "-b":
                        case "--bytecode":
                            engine.BytecodeInterpret(value, out int bcValue);
                            Console.WriteLine("Bytecode Interpreter returned " + bcValue);
                            break;
                        case "-to":
                            engine.TestOptimize(value);
                            break;
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
            builder.AppendLine("   mcc [-p | --verbose | -d | --debug] <source_file>");
            builder.AppendLine("   mcc [-v | --version]");
            builder.AppendLine("   mcc [-t | --test] [<stage_folder> | <source_file>]");
            builder.AppendLine("   mcc [-i | --interpret] <source_file>");
            builder.AppendLine("Options:");
            builder.AppendLine("   <source_file>                    Compile source file silently");
            builder.AppendLine("   [-p | --verbose] <source_file>   Compile source file with Verbose output");
            builder.AppendLine("   [-d | --debug] <source_file>     Compile source file with Debug output");
            builder.AppendLine("   [-v | --version]                 Print Version");
            builder.AppendLine("   [-t | --test] <stage_folder>     Test source files in stage folder");
            builder.AppendLine("   [-t | --test] <source_file>      Test single source file");
            builder.AppendLine("   [-i | --interpret] <source_file> Interpret single source file");
            Console.Write(builder.ToString());
        }
    }
}