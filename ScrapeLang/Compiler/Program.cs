using System;
using System.IO;
using Scrape.Cli;
using Scrape.Code.Generation;

namespace Scrape
{
    class Program
    {
        static void Main(string[] args) 
        {
            Global.Entrypoint = false;

			if (args.Length < 1) {
				Console.WriteLine("Usage: scrape <input>");

				return;
			}

			Console.WriteLine("Compiling");

            foreach (string file in Directory.GetFiles(Path.Combine(Directory.GetCurrentDirectory(), args[0])))
            {
                if (Path.GetExtension(file) == ".srp")
                {
                    LLVMCompiler compiler;

                    using (StreamReader reader = new StreamReader(file)) 
                    {
                        compiler = new LLVMCompiler(reader.ReadToEnd());
                    }

					compiler.Compile();

					// LLVMSharp.LLVM.DumpModule(compiler.Module);

                    /*compiler.Compile();

                    using (StreamWriter writer = new StreamWriter($"{Path.GetFileNameWithoutExtension(file)}.cpp")) 
                    {
                        writer.Write(compiler.Output);
                    }*/
                }
            }

            if (!Global.Entrypoint)
            {
                throw new CompileError("no entrypoint provided!");
            }
		}
    }
}