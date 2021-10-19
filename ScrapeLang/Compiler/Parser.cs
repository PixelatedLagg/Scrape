using System;
using System.Collections.Generic;

namespace Scrape {
	public enum ExprType {
		Literal,
		Binary
	}
	public class Expr {
		public ExprType Type;
		
		public Expr Left;
		public BFToken Op;
		public Expr Right;

		public BFToken Value;

		public Expr() {
			
		}

		public Expr(BFToken val) {
			Type = ExprType.Literal;
			Value = val;
		}
	}

	public class Parser {
		private Lexer Source;

		// Unfinished
		public Expr Primary() {
			BFToken tok = Source.GetBFToken();

			return new Expr(tok);
		}
		
		public Parser(string src) {
			Source = new Lexer(src);
		}
	}
}