using System.Linq;
using System;
using System.IO;

namespace Scrape
{
    static public class File
 	{
        static public void GetFile()
        {
            string[] files = Directory.GetFiles(Environment.CurrentDirectory, "*.srp").Concat(Directory.GetFiles(Environment.CurrentDirectory, "*.scrape")).ToArray();
            if (files.Length == 0)
            {
                throw new CompileError("No files found to compile!");
            }
            foreach (string file in files)
            {
                Console.WriteLine(file);
                new Parser(System.IO.File.ReadAllText(file));
            }
        }
    }
}