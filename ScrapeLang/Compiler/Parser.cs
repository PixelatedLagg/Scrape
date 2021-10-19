using System;
using System.Collections.Generic;

namespace Scrape {
	public enum ExprType {
		Literal,
		Binary
	}
	public class Expr {
		public ExprType Type;
		
		public Expr Right;

		public Token Value;

		public Expr() {
			
		}

		public Expr(Token val) {
			Type = ExprType.Literal;
			Value = val;
		}
	}

	public class Parser {
		private Lexer Source;

		// Unfinished
		public Expr Primary() {
			Token tok = Source.GetToken();

			return new Expr(tok);
		}
		
		public Parser(string src) {
			Source = new Lexer(src);
		}
	}
}