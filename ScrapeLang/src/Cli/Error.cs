using System;

namespace Scrape.Cli
{
    public static class Error
    {
        public static void ThrowError(string error)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Errors:\r\n{Global.CurrentPath} - {error}");
            Console.ForegroundColor = ConsoleColor.Gray;
            Environment.Exit(0);
        }
        public static void ThrowError(int line, int pos, string error)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Errors:\r\n{Global.CurrentPath}({line}, {pos}) - {error}");
            Console.ForegroundColor = ConsoleColor.Gray;
            Environment.Exit(0);
        }
        public static void CLIError(string error)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"CLI Error: {error}");
            Console.ForegroundColor = ConsoleColor.Gray;
            Environment.Exit(0);
        }
        public static void ThrowWarning(string warning)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"Warnings:\r\n{Global.CurrentPath} - {warning}");
            Console.ForegroundColor = ConsoleColor.Gray;
            Environment.Exit(0);
        }
        public static void ThrowWarning(int line, int pos, string warning)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"Warnings:\r\n{Global.CurrentPath}({line}, {pos}) - {warning}");
            Console.ForegroundColor = ConsoleColor.Gray;
            Environment.Exit(0);
        }
    }
}