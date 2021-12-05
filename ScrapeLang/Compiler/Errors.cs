using System.Linq;
using System;
using System.IO;
using System.Runtime.Serialization;

namespace Scrape
{
    public class SyntaxError : Exception
    {
        public string Position { get; set; }
        public SyntaxError(string message, Token token) : base($"Syntax Error: ({message}) Trace: {token}") 
        {
            Position = token.ToString();
        }
        protected SyntaxError(SerializationInfo info, StreamingContext context) : base(info, context) 
        { 
            info.AddValue("Position", Position, Position.GetType());
        }
    }
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
    }
}