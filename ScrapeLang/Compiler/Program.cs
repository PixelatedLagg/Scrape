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
            foreach (string file in Directory.GetFiles(Path.Combine(Directory.GetCurrentDirectory(), @"..\..")))
            {
                if (Path.GetExtension(file) == ".srp")
                {
                    Compiler compiler;
                    using (StreamReader reader = new StreamReader(file)) 
                    {
                        compiler = new Compiler(reader.ReadToEnd());
                    }
                    compiler.Compile();
                    using (StreamWriter writer = new StreamWriter($"{Path.GetFileNameWithoutExtension(file)}.cpp")) 
                    {
                        writer.Write(compiler.Output);
                    }
                }
            }
		}
    }
}