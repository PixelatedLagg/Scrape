using System.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using Scrape.Code.Generation;

namespace Scrape
{
    public static class Program
    {
        static void Main(string[] argsCLI) 
        {
            ProjectFile.GetProjectFile();
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
                                }
                                break;
                            case "--list":
                                Console.WriteLine($"Installed Scrape Templates: {Global.Templates.Count()}\r\nName:    Desc:    ID:");
                                foreach (Template template in Global.Templates)
                                {
                                    Console.WriteLine($"{template.Name}    {template.Desc}    {template.ID}");
                                }
                                break;
                            default:
                                Global.Templates = new List<Template>();
                                DefaultTemplates.Add();
                                ProjectFile.ReadTemplates();
                                foreach (Template template in Global.Templates)
                                {
                                    if (template.ID == args[2])
                                    {
                                        
                                    }
                                }
                                Error.CLIError($"\"{args[1]}\" is not an installed Scrape template ID");
                                break;
                        }
                        break;
                    case "run":
                        Global.EntryPoint = false;
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
                        if (!Global.EntryPoint)
                        {
                            Error.CLIError("No entrypoint specified");
                        }
                        //run generated files here
                        break;
                    case "build":
                        Global.EntryPoint = false;
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
                        if (!Global.EntryPoint)
                        {
                            Error.CLIError("No entrypoint specified");
                        }
                        //build generated files into exe here
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("Build Successful");
                        Console.ForegroundColor = ConsoleColor.Gray;
                        break;
                    default:
                        Console.WriteLine($"\"{args[0]}\" is not a valid command");
                        break;
                }
            }
		}
    }
}