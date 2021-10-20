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
    public class CompileError : Exception
    {
        public CompileError(string message) : base($"Compile Error: ({message})") {}
    }
    public class ParseException : Exception 
    {
		private string _Message;
		public override string Message { get { return _Message; } }
		public ParseException(Token tok, string message) 
        {
			_Message = $"({tok.Line}, {tok.Column}): {message}";
		}
		public ParseException(int line, int column, string message) 
        {
			_Message = $"({line}, {column}): {message}";
        }
    }
}