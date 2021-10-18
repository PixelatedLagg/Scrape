using System;
using System.Collections.Generic;
using System.Text;

namespace BFAsm
{
	public class ParseException : Exception {
		private string _Message;

		public override string Message { get { return _Message; } }


		public ParseException(BFToken tok, string message) {
			_Message = $"({tok.Line}, {tok.Column}): {message}";
		}

		public ParseException(int line, int column, string message) {
			_Message = $"({line}, {column}): {message}";
        }
    }

	public enum TokenType {
		Identifier,
		Keyword,
		Integer,
		Colon,
		Comma,
		String,
		LParen,
		RParen,
		Operator,
		EOF
	}

	public class BFToken {
		public TokenType Type;

		public int Line;

		public int Column;

		private object Value;
		
		/// <summary>
		/// Returns the value of the token as an integer.
		/// </summary>
		/// <returns><see cref="int"/></returns>
		public int Integer() {
			if (Type != TokenType.Integer) {
				throw new ParseException(Line, Column, "Token value is not an integer!");
			}

			return (int) Value;
		}

		/// <summary>
		/// Returns the value of the token as a string.
		/// </summary>
		/// <returns><see cref="string"/></returns>
		public string String() {
			/*if (Type != TokenType.Identifier && Type != TokenType.String) {
				throw new ParseException(Line, Column, "Token value is not a string!");
			}*/

			return Value.ToString();
		}

		public bool Is(TokenType type, object value) {
			return Type == type && Value.ToString() == value.ToString();
        }

        public override string ToString() {
			return $"({Line}, {Column}): {Type}, {Value}";
        }

        public BFToken(TokenType type, int line, int column, object value) {
			Type = type;

			Line = line;

			Column = column;

			Value = value;
        }
	}

	public LexerFLexer {
		/// <summary>
		/// Holds the code being tokenized.
		/// </summary>
		private string Source;

		private int Position = 0;

		private int Line = 1;

		private int Column = 1;

		public char Get()
		{
			if (Position >= Source.Length)
				return '\0';

			char c = Source[Position++];

			Column++;

			if (c == '\n') {
				Column = 1;

				Line++;
            }

			return c;
		}

		public char Peek()
		{
			if (Position >= Source.Length)
				return '\0';

			return Source[Position];
		}

		private bool IsAlpha(char c)
		{
			return (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || c == '_';
		}

		private bool IsDigit(char c)
		{
			return c >= '0' && c <= '9';
		}

		private bool IsSpace(char c) {
			return c == '\n' || c == '\t' || c == '\r' || c == ' ';
        }

		private bool IsOp(char c) {
			return c == '+' || c == '=' || c == '-' || c == '*' || c == '/';
        }

		private void SkipSpace() {
			while (IsSpace(Peek())) {
				Get();
            }
        }

		private string ReadUntil(Predicate<char> cmp, string expect) {
			string str = "";

			while (! cmp(Peek())) {
				if (Peek() == '\0') {
					throw new ParseException(Line, Column, $"Expected {expect} but got 'EOF'");
                }

				str += Get();
			}

			return str;
		}

		private string ReadWhile(Predicate<char> cmp) {
			string str = "";

			while (cmp(Peek())) {
				str += Get();
			}

			return str;
		}

		public BFToken GetToken() {
			SkipSpace();

			char c = Peek();

			if (c == '/') {
				while (Peek() != '\n') {
					Get();
				}

				Get();
			}

			if (IsAlpha(c)) {
				return new BFToken(TokenType.Identifier, Line, Column, ReadWhile(IsAlpha));
			}

			if (c == '"') {
				Get();

				BFToken tok = new BFToken(TokenType.String, Line, Column, ReadUntil(ch => ch == '"', "\""));

				Get();

				return tok;
			}

			if (c == ':') {
				Get();

				return new BFToken(TokenType.Colon, Line, Column, ":");
			}

			if (c == ',') {
				Get();

				return new BFToken(Tokeype.Comma, Line, Column, ",");
			}	}

			if (IsDigit(c)) {
				return new BFToken(TokenType.Integer, Line, Column, int.Parse(ReadWhile(IsDigit)));
            }

			if (IsOp(c)) {
				return new BFToken(TokenType.Operator, Line, Column, ReadWhile(IsOp));
			}

			return new BFToken(TokenType.EOF, Line, Column, null);
        }

		public BFToken PeekToken() {
			int pos = Position;

			BFToken tok = GetToken();

			Position = pos;

			return tok;
        }

		public Lexer(string source)
		{
			Source = source;
		}
	}
}
