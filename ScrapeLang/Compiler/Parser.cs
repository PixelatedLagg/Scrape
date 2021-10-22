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

		public Token Op;
		
		public Expr Right;

		public Token Value;

		public override string ToString() {
			if (Type == ExprType.Literal) {
				return Value.String();
			}

			string left = Left.Type == ExprType.Binary ? $"({Left})" : $"{Left}";

			string right = Right.Type == ExprType.Binary ? $"({Right})" : $"{Right}";

			return $"{left} {Op.String()} {right}";
		}

		public Expr() {
			
		}

		public Expr(Token val) {
			Type = ExprType.Literal;
			Value = val;
		}

		public Expr(Expr left, Token op, Expr right) {
			Type = ExprType.Binary;
			
			Left = left;

			Op = op;

			Right = right;
		}
	}

	public class Parser {
		private Lexer Source;

		private Stack<Expr> Operands = new Stack<Expr>();

		private class OperatorDescriptor {
			public string Op;

			public int Precedence;

			public bool LeftAssociative;

			public Token ToToken() {
				return new Token(TokenType.Operator, 0, 0, Op);
			}

			public OperatorDescriptor(string op, int prec, bool left) {
				Op = op;

				Precedence = prec;

				LeftAssociative = left;
			}
		}

		private Stack<OperatorDescriptor> Operators = new Stack<OperatorDescriptor>();

		private Dictionary<string, OperatorDescriptor> OperatorDefs = new Dictionary<string, OperatorDescriptor> {
			{ "*", new OperatorDescriptor("*", 7, true) },
			{ "/", new OperatorDescriptor("/", 6, true) }
		};

		public Expr Expression() {
			Operands.Push(Primary());

			while (Source.PeekToken().Type == TokenType.Operator && OperatorDefs.ContainsKey(Source.PeekToken().String())) {
				OperatorDescriptor op = OperatorDefs[Source.PeekToken().String()];

				if (Operators.Count > 0) {
					OperatorDescriptor next = Operators.Peek();
					
					while (Operators.Count > 0 && (next.Precedence < op.Precedence || (op.Precedence == next.Precedence && Operators.Peek().LeftAssociative))) {
						Expr right = Operands.Pop();

						Operands.Push(new Expr(Operands.Pop(), Operators.Pop().ToToken(), right));
					}
				}

				Source.GetToken();

				Operators.Push(op);

				Operands.Push(Primary());
			}

			while (Operators.Count > 0) {
				Expr right = Operands.Pop();

				Operands.Push(new Expr(Operands.Pop(), Operators.Pop().ToToken(), right));
			}

			return Operands.Pop();
		}

		/*public Expr Div() {
			Operands.Push(Primary());

			while (Source.PeekToken().Is(TokenType.Operator, "/")) {
				Operators.Push(Source.GetToken());

				Operands.Push(Primary());

				Expr right = Operands.Pop();

				Operands.Push(new Expr(Operands.Pop(), Operators.Pop(), right));
			}

			return Operands.Pop();
		}*/

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