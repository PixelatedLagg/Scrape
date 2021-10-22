using System;
using Scrape.Cli;

namespace Scrape
{
    class Program
    {
        static void Main(string[] args) {
			Parser prs = new Parser("10 * 6 / 2 / 2");

			Console.WriteLine(prs.Expression());
		} // => new Input().Start(args);
    }
}
