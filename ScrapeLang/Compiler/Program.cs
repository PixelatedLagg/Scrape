using System;
using Scrape.Cli;

namespace Scrape
{
    class Program
    {
        static void Main(string[] args) {
			Parser prs = new Parser("int thirty = 15 * 2");

			Console.WriteLine(prs.Statement());
		} // => new Input().Start(args);
    }
}
