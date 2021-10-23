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

	public enum StmtType {
		VarDef,
		If,
		Expression
	}

	// This could use inheritance with derived classes, but is the mess of checking types and casting worth it?
	public class Stmt {
		public StmtType Type;

		public string TypeName;

		public string Name;

		public Expr Condition;

		public Expr Expression;

		public List<Stmt> Body;

		public override string ToString() {
			if (Type == StmtType.Expression) {
				return Expression.ToString();
			}

			if (Type == StmtType.VarDef) {
				return $"{TypeName} {Name} = {Expression.ToString()}";
			}

			string str = $"if ({Condition.ToString()}) {{\n";

			foreach (Stmt st in Body) {
				str += '\t' + st.ToString() + '\n';
			}

			return str + '}';
		}

		public Stmt() {}

		public Stmt(Expr expr) {
			Type = StmtType.Expression;

			Expression = expr;
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
			{ "+", new OperatorDescriptor("+", 8, true) },
			{ "-", new OperatorDescriptor("-", 8, true) },
			{ "*", new OperatorDescriptor("*", 7, true) },
			{ "/", new OperatorDescriptor("/", 6, true) }
		};

		public Stmt Statement() {
			Stmt result = new Stmt();

			if (Source.PeekToken().Is(TokenType.Identifier, "if")) {
				Source.GetToken();

				if (Source.PeekToken().Type != TokenType.LParen) {
					throw new SyntaxError("Expected [(] after [if]", Source.GetToken());
				}

				Source.GetToken();

				Expr cond = Expression();

				if (Source.PeekToken().Type != TokenType.RParen) {
					throw new SyntaxError("Expected [)] after expression", Source.PeekToken());
				}

				Source.GetToken();

				result.Type = StmtType.If;

				result.Condition = cond;

				if (Source.PeekToken().Type != TokenType.LBracket) {
					throw new SyntaxError("Expected [{] after [)]", Source.PeekToken());
				}

				result.Body = Body();

				return result;
			}

			if (Source.PeekToken(3).Is(TokenType.Operator, "=")) {
				Token type = Source.GetToken();

				if (type.Type != TokenType.Identifier) {
					throw new SyntaxError("Expected identifier", type);
				}

				Token name = Source.GetToken();

				if (name.Type != TokenType.Identifier) {
					throw new SyntaxError("Expected identifier", name);
				}

				Source.GetToken(); // =

				result.Type = StmtType.VarDef;

				result.TypeName = type.String();

				result.Name = name.String();

				result.Expression = Expression();

				return result;
			}

			return new Stmt(Expression());
		}

		public List<Stmt> Body() {
			Token start = Source.GetToken(); // {
			
			List<Stmt> statements = new List<Stmt>();
			
			while (Source.PeekToken().Type != TokenType.RBracket) {
				if (Source.PeekToken().Type == TokenType.EOF) {
					throw new SyntaxError("Expected [}] before end of file.", start);
				}

				statements.Add(Statement());
			}

			Source.GetToken(); // }

			return statements;
		}

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

			if (tok.Type == TokenType.LParen) {
				Expr expr = Expression();

				if (Source.PeekToken().Type != TokenType.RParen) {
					throw new SyntaxError("Expected [)] after expression.", tok);
				}

				Source.GetToken();

				return expr;
			}

			return new Expr(tok);
		}
		
		public Parser(string src) {
			Source = new Lexer(src);
		}
	}
}