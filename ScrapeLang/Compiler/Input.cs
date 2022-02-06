using System;
using Scrape;

namespace Scrape.Cli
{
    public class Input
    {
        public void Start(string[] args)
        {
            if (args.Length == 1)
            {
                switch (args[0])
                {
                    case "run":
                        //run
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