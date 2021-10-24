using System;
using System.IO;
using Scrape.Cli;
using Scrape.Code.Generation;

namespace Scrape
{
    class Program
    {
        static void Main(string[] args) {
            Compiler compiler = new Compiler(File.ReadAllText("Program.srp"));

            compiler.Compile();

            // Write gen.Namespace(prs.TopLevel()) to generated.cpp
            using (StreamWriter writer = new StreamWriter("generated.cpp")) {
                writer.Write(compiler.Output);
            }

		} // => new Input().Start(args);
    }
}
