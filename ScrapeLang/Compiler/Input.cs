using System;
using Scrape;

namespace Scrape.Cli
{
    public class Input
    {
        public void Start()
        {
            while (true)
            {
                RegisterInput(Console.ReadLine());
            }
        }
        private void RegisterInput(string input)
        {
            switch (input)
            {
                case "scrape run":
                    //run
                    break;
                case "scrape exit":
                    Environment.Exit(0);
                    break;
                default:
                    throw new CompileError($"\"{input}\" is not a valid command!");
            }
        }
    }
}