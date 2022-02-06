using System;
using System.Collections.Generic;
using System.Text;

namespace Scrape
{
	public enum TokenType {
		Identifier,
		Keyword,
		Integer,
		Colon,
		Semicolon,
		Comma,
		String,
		LParen,
		RParen,
		LBrace,
		RBrace,
		LBracket,
		RBracket,
		Operator,
		EOF
	}

	public class Token {
		public TokenType Type;

		public int Line;

		public int Column;

		private object Value;
		
		/// <summary>
		/// Returns the value of the Token as an integer.
		/// </summary>
		/// <returns><see cref="int"/></returns>
		public int Integer() {
			if (Type != TokenType.Integer) {
				throw new ParseException(Line, Column, "Token value is not an integer!");
			}

			return (int) Value;
		}

		/// <summary>
		/// Returns the value of the Token as a string.
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

        public Token(TokenType type, int line, int column, object value) {
			Type = type;

			Line = line;

			Column = column;

			Value = value;
        }
	}

	public class Lexer {
		/// <summary>
		/// Holds the code being Tokenized.
		/// </summary>
		private string Source;

		public int Position = 0;

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

		public Token GetToken() {
			SkipSpace();

			char c = Peek();

			if (c == '/' && Source[Position + 1] == '/') {
				while (Peek() != '\n') {
					Get();
				}

				Get();
			}

			if (IsAlpha(c)) {
				return new Token(TokenType.Identifier, Line, Column, ReadWhile(IsAlpha));
			}

			if (c == '"') {
				Get();

				Token tok = new Token(TokenType.String, Line, Column, ReadUntil(ch => ch == '"', "\""));

				Get();

				return tok;
			}

			if (c == '.') {
				Get();

				return new Token(TokenType.Operator, Line, Column, ".");
			}

			if (c == ':') {
				Get();

				return new Token(TokenType.Colon, Line, Column, ":");
			}

			if (c == ';') {
				Get();

				return new Token(TokenType.Semicolon, Line, Column, ";");
			}

			if (c == '(') {
				Get();

				return new Token(TokenType.LParen, Line, Column, "(");
			}

			if (c == ')') {
				Get();

				return new Token(TokenType.RParen, Line, Column, ")");
			}

			if (c == '{') {
				Get();

				return new Token(TokenType.LBracket, Line, Column, "{");
			}

			if (c == '}') {
				Get();

				return new Token(TokenType.RBracket, Line, Column, "}");
			}

			if (c == '[') {
				Get();

				return new Token(TokenType.LBrace, Line, Column, "[");
			}

			if (c == ']') {
				Get();

				return new Token(TokenType.RBrace, Line, Column, "]");
			}

			if (c == ',') {
				Get();

				return new Token(TokenType.Comma, Line, Column, ",");
			}	
			if (IsDigit(c)) {
				return new Token(TokenType.Integer, Line, Column, int.Parse(ReadWhile(IsDigit)));
            }

			if (IsOp(c)) {
				return new Token(TokenType.Operator, Line, Column, ReadWhile(IsOp));
			}

			return new Token(TokenType.EOF, Line, Column, null);
        }

		public Token PeekToken(int count = 1) {
			int pos = Position;

			int line = Line;

			int col = Column;

			Token tok = GetToken();

			for (int i = 1; i < count; i++) {
				tok = GetToken();
			}

			Position = pos;

			Line = line;

			Column = col;

			return tok;
        }

		public Lexer(string source)
		{
			Source = source;
		}
	}
}
