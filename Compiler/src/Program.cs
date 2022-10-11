using System;
using CLICarry;
using System.IO;
using Scrape.Code.Generation;

namespace Scrape
{
    class Program
    {
        static void Main(string[] args) 
        {
            CLIManager.Run<MainCLI>(args);
		}
    }
    class MainCLI : CLI
    {
        void CLI.Error(CLICarry.ErrorContext context) { }

        [Command("run")]
        public void Run(CommandArgs args)
        {
            foreach (string file in Directory.GetFiles(Directory.GetCurrentDirectory(), "*.srp", SearchOption.AllDirectories))
            {
                //compile
            }
        }
    }
}