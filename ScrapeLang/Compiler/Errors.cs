using System.Linq;
using System;
using System.IO;

namespace Scrape
{
    class SyntaxError : Exception
    {
        public SyntaxError(string message, Token token) : base($"Syntax Error: {message} Trace: {token.ToString()}") {}
    }
    class CompileError : Exception
    {
        public CompileError(string message) : base($"Compile Error: {message}") {}
    }
}    