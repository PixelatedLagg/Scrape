using System;
using System.Collections.Generic;
using System.IO;
using Scrape.Code.Generation;

namespace Scrape
{
    class Program
    {
        static void Main(string[] argsCLI) 
        {
            List<string> argsList = new List<string>();
            foreach (string s in argsCLI)
            {   
                argsList.Add(s.ToLower());
            }
            string[] args = argsList.ToArray();
            if (args.Length == 1)
            {
                switch (args[0])
                {
                    case "new":
                        switch (args[1])
                        {
                            case "console":
                                if (File.Exists($"{Path.Combine(Directory.GetCurrentDirectory(), @"..\..")}\\Program.srp"))
                                {
                                    Console.WriteLine("This operation will overwrite Program.srp\r\n[Y] or [N] to continue");
                                    if (Console.ReadLine().ToLower() == "y")
                                    {
                                        using (StreamWriter sw = new StreamWriter($"{Path.Combine(Directory.GetCurrentDirectory(), @"..\..")}\\Program.srp"))
                                        {
                                            sw.Write("class Program\r\n{\r\n    public static void Main()\r\n    {        \r\n    {\r\n{");
                                        }
                                    }
                                    Environment.Exit(0);
                                }
                                break;
                            default:
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine($"\"{args[1]}\" is not a Scrape CLI template");
                                Console.ForegroundColor = ConsoleColor.Gray;
                                break;
                        } //add --list option to list all templates
                        break;
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
                        //run generated files here
                        Environment.Exit(0);
                        break;
                    case "build":
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
                        //build generated files into exe here
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("Build Successful");
                        Console.ForegroundColor = ConsoleColor.Gray;
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