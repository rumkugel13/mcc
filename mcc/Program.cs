using System.Runtime.InteropServices;
using System.Text;

namespace mcc
{
    internal class Program
    {
        static string VersionString = "mcc v0.16";

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
                    else if (args[0].Equals("-v"))
                    {
                        PrintVersion();
                    }
                    break;
                case 2:
                    string argument = args[0];
                    string value = args[1];

                    if (argument.Equals("-t"))
                    {
                        engine.Silent = false;

                        string path = value;
                        if (path.EndsWith(".c"))
                        {
                            if (!File.Exists(path))
                            {
                                Console.WriteLine("Unknown file: " + path);
                                return;
                            }

                            engine.TestOne(path);
                        }
                        else
                        {
                            if (!Directory.Exists(path))
                            {
                                Console.WriteLine("Unknown folder: " + path);
                                return;
                            }

                            engine.TestAll(path);
                        }
                    }
                    else if (argument.Equals("-p"))
                    {
                        engine.Silent = false;
                        string filePath = value;
                        engine.Compile(filePath);
                    }
                    else if (argument.Equals("-d"))
                    {
                        engine.Silent = false;
                        engine.Debug = true;
                        string filePath = value;
                        engine.Compile(filePath);
                    }
                    else if (argument.Equals("-i"))
                    {
                        engine.Silent = true;
                        engine.Debug = false;
                        string filePath = value;
                        engine.Interpret(filePath, out int interpreted);
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
    }
}