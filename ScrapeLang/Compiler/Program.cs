using System;
using System.IO;
using Scrape.Code.Generation;

namespace Scrape
{
    class Program
    {
        static void Main(string[] args) 
        {
            if (args.Length == 1)
            {
                switch (args[0])
                {
                    case "run":
                        Global.Entrypoint = false;
                        Directory.CreateDirectory($"{Path.Combine(Directory.GetCurrentDirectory(), @"..\..")}\\output");
                        foreach (string file in Directory.GetFiles(Path.Combine(Directory.GetCurrentDirectory(), @"..\..")))
                        {
                            if (Path.GetExtension(file) == ".srp")
                            {
                                Global.CurrentPath = Path.GetFullPath(file);
                                Compiler compiler;
                                using (StreamReader reader = new StreamReader(file)) 
                                {
                                    compiler = new Compiler(reader.ReadToEnd());
                                }
                                compiler.Compile();
                                using (StreamWriter writer = new StreamWriter($"{Path.Combine(Directory.GetCurrentDirectory(), @"..\..")}\\output\\{Path.GetFileNameWithoutExtension(file)}.cpp")) 
                                {
                                    writer.Write(compiler.Output);
                                }
                            }
                        }
                        if (!Global.Entrypoint)
                        {
                            Error.ThrowError("No entrypoint specified!");
                        }
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("successful!");
                        Console.ForegroundColor = ConsoleColor.Gray;
                        Environment.Exit(0);
                        break;
                    case "build":
                        //build
                        break;
                    default:
                        Console.WriteLine($"\"{args[0]}\" is not a valid command!");
                        Environment.Exit(0);
                        break;
                }
            }
		}
    }
}