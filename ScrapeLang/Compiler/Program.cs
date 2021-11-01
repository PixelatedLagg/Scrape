using System;
using System.IO;
using Scrape.Cli;
using Scrape.Code.Generation;

namespace Scrape
{
    class Program
    {
        static void Main(string[] args) {
            Compiler compiler;
            using (StreamReader reader = new StreamReader("Program.srp")) {
                compiler = new Compiler(reader.ReadToEnd());
            }
            compiler.Compile();
            using (StreamWriter writer = new StreamWriter("generated.cpp")) {
                writer.Write(compiler.Output);
            }
		} // => new Input().Start(args);
    }
}