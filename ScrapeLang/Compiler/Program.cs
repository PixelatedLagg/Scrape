using System;
using System.IO;
using Scrape.Code.Generation;

namespace Scrape
{
    class Program
    {
        static void Main(string[] args) 
        {
            Global.Entrypoint = false;
			if (args.Length < 1) 
            {
				Console.WriteLine("Invalid command. Use <scrape help> to list all commands.");
				return;
			}
            switch (args[0])
            {
                case "run":
                    foreach (string file in Directory.GetFiles(Directory.GetCurrentDirectory(), "*.srp", SearchOption.AllDirectories))
                    {
                        /*
                        LLVMCompiler compiler;
                        using (StreamReader reader = new StreamReader(file)) 
                        {
                            compiler = new LLVMCompiler(reader.ReadToEnd());
                        }
                        compiler.Compile();
                        LLVMSharp.LLVM.DumpModule(compiler.Module);
                        compiler.Compile();
                        using (StreamWriter writer = new StreamWriter($"{Path.GetFileNameWithoutExtension(file)}.cpp")) 
                        {
                            writer.Write(compiler.Output);
                        }
                        */
                    }
                    break;
                case "list":
                    if (args.Length < 2)
                    {
                        Console.WriteLine("Must supply at least one file to compile.");
                        return;
                    }
                    for (int x = 2; x < args.Length; x++)
                    {
                        if (Path.GetExtension(args[x]) != ".srp")
                        {
                            Console.WriteLine("Invalid file type.");
                            return;
                        }
                        /*
                        LLVMCompiler compiler;
                        using (StreamReader reader = new StreamReader(args[x])) 
                        {
                            compiler = new LLVMCompiler(reader.ReadToEnd());
                        }
                        compiler.Compile();
                        LLVMSharp.LLVM.DumpModule(compiler.Module);
                        compiler.Compile();
                        using (StreamWriter writer = new StreamWriter($"{Path.GetFileNameWithoutExtension(file)}.cpp")) 
                        {
                            writer.Write(compiler.Output);
                        }
                        */
                    }
                    break;
                case "help":
                    Console.WriteLine("Commands:^scrape run - Recursively search current directory and all sub directories to compile Scrape files.^scrape list <file(s)> - Compile all listed Scrape files.");
                    break;
            }
		}
    }
}