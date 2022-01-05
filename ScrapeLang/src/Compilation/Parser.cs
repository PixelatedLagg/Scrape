using System;
using System.Collections.Generic;
using Scrape.Cli;

namespace Scrape.Code.Generation 
{
	public enum ExprType 
	{
		Literal,
		Call,
		Index,
		Binary,
		New
	}
	public class Expr 
	{
		public ExprType Type;
		public Expr Left;
		public Token Op;
		public Expr Right;
		public Token Value;
		public Expr Subject;
		public Expr Index;
		public List<Expr> Args;
		public override string ToString() 
		{
			switch (Type)
			{
				case ExprType.Literal:
					if (Value.Type == TokenType.String) 
					{
						return $"\"{Value.String()}\"";
					}
					return Value.String();
				case ExprType.Call:
					return $"{Subject.ToString()}({String.Join(", ", Args)})";
				case ExprType.Index:
					return $"{Subject.ToString()}[{Index.ToString()}]";
			}
			string left = Left.Type == ExprType.Binary && Left.Op.String() != "." ? $"({Left})" : $"{Left}";
			string right = Right.Type == ExprType.Binary && Right.Op.String() != "." ? $"({Right})" : $"{Right}";
			return Op.String() == "." ? $"{left}{Op.String()}{right}" : $"{left} {Op.String()} {right}";
		}
		public Expr() {}
		public Expr(Token val) 
		{
			Type = ExprType.Literal;
			Value = val;
		}
		public Expr(Expr subject, List<Expr> args) 
		{
			Type = ExprType.Call;
			Subject = subject;
			Args = args;
		}
		public Expr(Expr left, Token op, Expr right) 
		{
			Type = ExprType.Binary;
			Left = left;
			Op = op;
			Right = right;
		}
	}
	public enum StmtType 
	{
		VarDef,
		Assignment,
		If,
		Return,
		Expression,
		Method
	}
	// This could use inheritance with derived classes, but is the mess of checking types and casting worth it?
	public class Stmt 
	{
		public StmtType Type;
		public List<string> Modifiers = new List<string>();
		public string TypeName;
		public string Name;
		public Expr Path;
		public Expr Condition;
		public Expr Expression;
		public List<Stmt> Body;
		public override string ToString() 
		{
			switch (Type)
			{
				case StmtType.Expression:
					return Expression.ToString();
				case StmtType.Return:
					return $"return {Expression.ToString()}";
				case StmtType.VarDef:
					return $"{TypeName} {Name} = {Expression.ToString()}";
				case StmtType.Method:
					string mstr = $"{TypeName} {Name}() {{\n";
					foreach (Stmt st in Body) 
					{
						mstr += '\t' + st.ToString() + '\n';
					}
					return mstr + '}';
			}
			string str = $"if ({Condition.ToString()}) {{\n";
			foreach (Stmt st in Body) 
			{
				str += '\t' + st.ToString() + '\n';
			}
			return str + '}';
		}
		public Stmt() {}
		public Stmt(Expr expr) 
		{
			Type = StmtType.Expression;
			Expression = expr;
		}
	}
	public enum ClassMemberType 
	{
		Field,
		Method
	}
	// Class to represent class members, needs to hold the name of the class it belongs to, and the type of member, as well as body if it is a method(list of Stmt classes)
	public class ClassMember 
	{
		public ClassMemberType Type;
		public string Name;
		public string TypeName;
		public List<string> Modifiers = new List<string>();
		public Expr Expression;
		public List<Stmt> Body;
		public List<string> ArgNames = new List<string>();
		public List<Expr> ArgTypes = new List<Expr>();
		public override string ToString() {
			// Convert member to string
			switch (Type)
			{
				case ClassMemberType.Method:
					string mstr = $"{TypeName} {Name}(";
					mstr += "{{\n";
					for (int i = 0; i < ArgNames.Count; i++) 
					{
						mstr += $"\t{ArgTypes[i].ToString()} {ArgNames[i]}{(i == ArgNames.Count - 1 ? "" : ", ")}\n";
					}
					TopLevel.IndentLevel++;
					foreach (Stmt st in Body) 
					{
						// Indent with static variable from TopLevel class
						for (int i = 0; i < TopLevel.IndentLevel; i++) 
						{
							mstr += '\t';
						}
						mstr += st.ToString() + '\n';
					}
					TopLevel.IndentLevel--;
					for (int i = 0; i < TopLevel.IndentLevel; i++) 
					{
						mstr += '\t';
					}
					return mstr + '}';
				case ClassMemberType.Field:
					return $"{TypeName} {Name} = {Expression.ToString()}";
			}
			return "";
		}
		public ClassMember() {}
	}
	public enum TopLevelType 
	{
		Namespace,
		Class,
		Using
	}
	public class TopLevel 
	{
		public TopLevelType Type;
		public string Name;
		public Expr Path; // using Path.To.Namespace;
		public List<string> Modifiers = new List<string>();
		public List<ClassMember> ClassData = new List<ClassMember>();
		public List<TopLevel> NamespaceData = new List<TopLevel>();
		public static int IndentLevel = 0;
		public override string ToString() 
		{
			switch (Type)
			{
				case TopLevelType.Namespace:
					string str = $"namespace {Name} {{\n";
					IndentLevel++;
					foreach (TopLevel tl in NamespaceData) 
					{
						for (int i = 0; i < IndentLevel; i++) 
						{
							str += '\t';
						}
						str += tl.ToString() + '\n';
					}
					IndentLevel--;
					// Indent the bracket
					for (int i = 0; i < IndentLevel; i++) {
						str += '\t';
					}
					return str + '}';
				case TopLevelType.Class:
					string str2 = $"class {Name} {{\n";
					IndentLevel++;
					foreach (ClassMember cm in ClassData) 
					{
						// Add correct indentation level to class member dependent on current indentation
						for (int i = 0; i < IndentLevel; i++) 
						{
							str2 += '\t';
						}
						str2 += cm.ToString() + '\n';
					}
					IndentLevel--;
					// Indent the bracket
					for (int i = 0; i < IndentLevel; i++) 
					{
						str2 += '\t';
					}
					return str2 + '}';
				case TopLevelType.Using:
					return $"using {Path.ToString()};";
			}
			return "";
		}
		public TopLevel() {}
	}
	public class Parser 
	{
		private Lexer Source;
		private Stack<Expr> Operands = new Stack<Expr>();
		private class OperatorDescriptor 
		{
			public string Op;
			public int Precedence;
			public bool LeftAssociative;
			public Token ToToken() => new Token(TokenType.Operator, 0, 0, Op);
			public OperatorDescriptor(string op, int prec, bool left) 
			{
				Op = op;
				Precedence = prec;
				LeftAssociative = left;
			}
		}
		private Stack<OperatorDescriptor> Operators = new Stack<OperatorDescriptor>();
		private Dictionary<string, OperatorDescriptor> OperatorDefs = new Dictionary<string, OperatorDescriptor> 
		{
			{ "==", new OperatorDescriptor("==", 9, true) },
			{ "+", new OperatorDescriptor("+", 8, true) },
			{ "-", new OperatorDescriptor("-", 8, true) },
			{ "*", new OperatorDescriptor("*", 7, true) },
			{ "/", new OperatorDescriptor("/", 6, true) },
			{ ".", new OperatorDescriptor(".", 5, true) }
		};
		private List<string> Modifiers = new List<string>() { "public", "private", "static", "abstract", "extern" };
		public TopLevel TopLevel() 
		{
			TopLevel result = new TopLevel();
			// Get modifiers
			while (Source.PeekToken().Type == TokenType.Identifier && Modifiers.Contains(Source.PeekToken().String())) 
			{
				result.Modifiers.Add(Source.GetToken().String());
			}
			// Namespace
			if (Source.PeekToken().Is(TokenType.Identifier, "namespace")) 
			{
				Source.GetToken(); // namespace
				result.Type = TopLevelType.Namespace;
				result.Name = Source.GetToken().String();
				if (Source.PeekToken().Type != TokenType.LBracket) 
				{
					Error.ThrowError(Source.GetLine, Source.GetColumn, "[SRP2001] Expected [{] after namespace definition");
				}
				Source.GetToken(); // {
				// Get namespace contents
				while (Source.PeekToken().Type != TokenType.RBracket) 
				{
					if (Source.PeekToken().Type == TokenType.EOF) 
					{
						Error.ThrowError(Source.GetLine, Source.GetColumn, "[SRP2002] Expected [}] after namespace body");
					}
					result.NamespaceData.Add(TopLevel());
				}
				Source.GetToken(); // }
				return result;
			}
			if (Source.PeekToken().Is(TokenType.Identifier, "class")) 
			{
				Source.GetToken();
				Token name = Source.GetToken();
				if (name.Type != TokenType.Identifier) 
				{
					Error.ThrowError(Source.GetLine, Source.GetColumn, "[SRP2003] Expected identifier after class definition");
				}
				result.Type = TopLevelType.Class;
				result.Name = name.String();
				// Error if next token is not a {
				if (Source.PeekToken().Type != TokenType.LBracket) 
				{
					Error.ThrowError(Source.GetLine, Source.GetColumn, "[SRP2004] Expected [{] after class definition");
				}
				result.ClassData = ClassBody();
				return result;
			}
			if (Source.PeekToken().Is(TokenType.Identifier, "using")) 
			{
				Source.GetToken();
				result.Type = TopLevelType.Using;
				result.Path = Expression();
				if (Source.PeekToken().Type != TokenType.Semicolon) 
				{
					Error.ThrowError(Source.GetLine, Source.GetColumn, "[SRP2005] Expected [;] after using statement");
				}
				Source.GetToken(); // ;
				return result;
			}
			return null;
		}
		public Stmt Statement() 
		{
			Stmt result = new Stmt();
			if (Source.PeekToken().Is(TokenType.Identifier, "return")) 
			{
				Source.GetToken(); // return
				result.Type = StmtType.Return;
				result.Expression = Expression();
				if (Source.PeekToken().Type != TokenType.Semicolon) 
				{
					Error.ThrowError(Source.GetLine, Source.GetColumn, "[SRP2006] Expected [;] after return statement");
				}
				Source.GetToken(); // ;
				return result;
			}
			if (Source.PeekToken().Is(TokenType.Identifier, "if")) 
			{
				Source.GetToken();
				if (Source.PeekToken().Type != TokenType.LParen) 
				{
					Error.ThrowError(Source.GetLine, Source.GetColumn, "[SRP2007] Expected [(] after [if]");
				}
				Source.GetToken();
				Expr cond = Expression();
				if (Source.PeekToken().Type != TokenType.RParen) 
				{
					Error.ThrowError(Source.GetLine, Source.GetColumn, "[SRP2008] Expected [)] after expression");
				}
				Source.GetToken();
				result.Type = StmtType.If;
				result.Condition = cond;
				if (Source.PeekToken().Type != TokenType.LBracket) 
				{
					Error.ThrowError(Source.GetLine, Source.GetColumn, "[SRP2009] Expected [{] after [)]");
				}
				result.Body = Body();
				return result;
			}
			int pos = Source.Position;
			Expr path = Expression(true);
			if (Source.PeekToken().Is(TokenType.Operator, "=")) 
			{
				if (Source.PeekToken().Type != TokenType.Operator || Source.PeekToken().String() != "=") 
				{
					Error.ThrowError(Source.GetLine, Source.GetColumn, "[SRP2010] Expected [=] after assignment subject");
				}
				Source.GetToken(); // =
				result.Type = StmtType.Assignment;
				result.Path = path;
				result.Expression = Expression();
				if (Source.PeekToken().Type != TokenType.Semicolon) 
				{
					Error.ThrowError(Source.GetLine, Source.GetColumn, "[SRP2011] Expected [;] after assignment");
				}
				Source.GetToken(); // ;
				return result;
			}
			Source.Position = pos;
			if (Source.PeekToken(3).Is(TokenType.Operator, "=")) 
			{
				Token type = Source.GetToken();
				if (type.Type != TokenType.Identifier) 
				{
					Error.ThrowError(Source.GetLine, Source.GetColumn, "[SRP2012] Expected identifier");
				}
				Token name = Source.GetToken();
				if (name.Type != TokenType.Identifier) 
				{
					Error.ThrowError(Source.GetLine, Source.GetColumn, "[SRP2012] Expected identifier");
				}
				Source.GetToken(); // =
				result.Type = StmtType.VarDef;
				result.TypeName = type.String();
				result.Name = name.String();
				result.Expression = Expression();
				if (Source.GetToken().Type != TokenType.Semicolon) 
				{
					Error.ThrowError(Source.GetLine, Source.GetColumn, "[SRP2013] Expected [;] after statement");
				}
				return result;
			}
			List<string> modifiers = new List<string>();
			if (Source.PeekToken().Type == TokenType.Identifier && Modifiers.Contains(Source.PeekToken().String())) 
			{
				modifiers.Add(Source.GetToken().String()); // Add multiple modifier support later
			}
			if (Source.PeekToken(3).Is(TokenType.LParen, "(")) 
			{
				Token tname = Source.GetToken();
				if (tname.Type != TokenType.Identifier)
				{
					Error.ThrowError(Source.GetLine, Source.GetColumn, "[SRP2012] Expected identifier");
				}
				Token name = Source.GetToken();
				if (name.Type != TokenType.Identifier) 
				{
					Error.ThrowError(Source.GetLine, Source.GetColumn, "[SRP2012] Expected identifier");
				}
				if (Source.PeekToken().Is(TokenType.LParen, "(")) 
				{
					result.Type = StmtType.Method;
					result.TypeName = tname.String();
					result.Name = name.String();
					result.Modifiers = modifiers;
					Source.GetToken(); // (
					if (!Source.PeekToken().Is(TokenType.RParen, ")")) 
					{
						Error.ThrowError(Source.GetLine, Source.GetColumn, "[SRP2014] Expected [)] after argument list");
					}
					Source.GetToken(); // )
					if (!Source.PeekToken().Is(TokenType.LBracket, "{")) 
					{
						Error.ThrowError(Source.GetLine, Source.GetColumn, "[SRP2015] Expected [{] after argument list");
					}
					result.Body = Body();
					if (Source.GetToken().Type != TokenType.Semicolon) 
					{
						Error.ThrowError(Source.GetLine, Source.GetColumn, "[SRP2013] Expected [;] after statement");
					}
					return result;
				}
			}
			Expr expr = Expression();
			// Check if next token is a ( and if it is, handle the expression as a method call
			/*if (Source.PeekToken().Is(TokenType.LParen, "(")) {
				Source.GetToken(); // (

				List<Expr> Args = new List<Expr>();

				if (! Source.PeekToken().Is(TokenType.RParen, ")")) {
					Args.Add(Expression());

					while (Source.PeekToken().Is(TokenType.Comma, ",")) {
						Source.GetToken(); // ,

						Args.Add(Expression());
					}
				}

				if (! Source.PeekToken().Is(TokenType.RParen, ")")) {
					throw new SyntaxError("Expected [)] after argument list", Source.GetToken());
				}

				Source.GetToken(); // )

				if (Source.GetToken().Type != TokenType.Semicolon) {
					throw new SyntaxError("Expected [;] after statement", Source.GetToken());
				}

				return new Stmt(new Expr(expr, Args));
			}*/

			if (Source.GetToken().Type != TokenType.Semicolon) 
			{
				Error.ThrowError(Source.GetLine, Source.GetColumn, "[SRP2013] Expected [;] after statement");
			}
			return new Stmt(expr);
		}
		public List<ClassMember> ClassBody() 
		{
			Source.GetToken(); // {
			List<ClassMember> result = new List<ClassMember>();
			while (Source.PeekToken().Type != TokenType.RBracket) 
			{
				result.Add(ClassMember());
			}
			Source.GetToken(); // }
			return result;
		}
		public ClassMember ClassMember() 
		{
			ClassMember result = new ClassMember();
			// Check if next tokens are a modifier and add it to modifiers of result
			while (Source.PeekToken().Type == TokenType.Identifier && Modifiers.Contains(Source.PeekToken().String())) 
			{
				result.Modifiers.Add(Source.GetToken().String());
			}
			// type name = value
			if (Source.PeekToken().Type == TokenType.Identifier) 
			{
				Token type = Source.GetToken();
				Token name = Source.GetToken();
				if (name.Type != TokenType.Identifier) 
				{
					Error.ThrowError(Source.GetLine, Source.GetColumn, "[SRP2012] Expected identifier");
				}
				if (Source.PeekToken().Is(TokenType.Operator, "=")) 
				{
					Source.GetToken(); // =
					result.Type = ClassMemberType.Field;
					result.TypeName = type.String();
					result.Name = name.String();
					result.Expression = Expression();
					return result;
				}
				if (Source.PeekToken().Is(TokenType.LParen, "(")) 
				{
					result.Type = ClassMemberType.Method;
					result.TypeName = type.String();
					result.Name = name.String();
					Token open = Source.GetToken(); // (
					while (! Source.PeekToken().Is(TokenType.RParen, ")")) 
					{
						if (Source.PeekToken().Type == TokenType.EOF) 
						{
							Error.ThrowError(Source.GetLine, Source.GetColumn, "[SRP2014] Expected [)] after argument list");
						}
						// Add name = type
						Expr tname = Expression();
						Token name2 = Source.GetToken();
						if (name2.Type != TokenType.Identifier) 
						{
							Error.ThrowError(Source.GetLine, Source.GetColumn, "[SRP2012] Expected identifier");
						}
						result.ArgNames.Add(name2.String());
						result.ArgTypes.Add(tname);
						if (Source.PeekToken().Is(TokenType.Comma, ",")) 
						{
							Source.GetToken(); // ,
						}
					}
					Source.GetToken(); // )
					bool nobody = result.Modifiers.Contains("abstract") || result.Modifiers.Contains("extern");
					if (! nobody && ! Source.PeekToken().Is(TokenType.LBracket, "{")) 
					{
						Error.ThrowError(Source.GetLine, Source.GetColumn, "[SRP2015] Expected [{] after argument list");
					}
					if (!nobody) 
					{
						result.Body = Body();
					}
					else 
					{
						if (Source.PeekToken().Type != TokenType.Semicolon) 
						{
							Error.ThrowError(Source.GetLine, Source.GetColumn, "[SRP2016] Expected [;] after extern function");
						}
						Source.GetToken(); // ;
					}
					return result;
				}
			}
			return result;
		}
		public List<Stmt> Body() 
		{
			Token start = Source.GetToken(); // {
			List<Stmt> statements = new List<Stmt>();
			while (Source.PeekToken().Type != TokenType.RBracket) 
			{
				if (Source.PeekToken().Type == TokenType.EOF) 
				{
					Error.ThrowError(Source.GetLine, Source.GetColumn, "[SRP2017] Expected [}] before end of file");
				}
				statements.Add(Statement());
			}
			Source.GetToken(); // }
			return statements;
		}
		public Expr Expression(bool dot = false) 
		{
			Operands.Push(Primary());
			while (Source.PeekToken().Type == TokenType.Operator && OperatorDefs.ContainsKey(Source.PeekToken().String())) 
			{
				OperatorDescriptor op = OperatorDefs[Source.PeekToken().String()];
				if (Operators.Count > 0) 
				{
					OperatorDescriptor next = Operators.Peek();
					while (Operators.Count > 0 && (next.Precedence < op.Precedence || (op.Precedence == next.Precedence && Operators.Peek().LeftAssociative))) 
					{
						Expr right = Operands.Pop();
						Operands.Push(new Expr(Operands.Pop(), Operators.Pop().ToToken(), right));
					}
				}
				Source.GetToken();
				if (dot && op.Op != ".") 
				{
					Error.ThrowError(Source.GetLine, Source.GetColumn, "[SRP2018] Expected [.] after identifier");
				}
				Operators.Push(op);
				Operands.Push(Primary());
			}
			while (Operators.Count > 0) 
			{
				Expr right = Operands.Pop();
				Operands.Push(new Expr(Operands.Pop(), Operators.Pop().ToToken(), right));
			}
			Expr expr = Operands.Pop();
			if (Source.PeekToken().Type == TokenType.LParen && !dot) 
			{
				Source.GetToken(); // (
				List<Expr> args = new List<Expr>();
				if (! Source.PeekToken().Is(TokenType.RParen, ")")) 
				{
					args.Add(Expression());
					while (Source.PeekToken().Is(TokenType.Comma, ",")) 
					{
						if (Source.PeekToken().Type == TokenType.EOF) 
						{
							Error.ThrowError(Source.GetLine, Source.GetColumn, "[SRP2014] Expected [)] after argument list");
						}
						Source.GetToken(); // ,
						args.Add(Expression());
					}
				}
				Source.GetToken(); // )
				return new Expr(expr, args);
			}
			return expr;
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
		public Expr Primary() 
		{
			Token tok = Source.GetToken();
			//Expr result;
			if (tok.Is(TokenType.Identifier, "new")) 
			{
				if (Source.PeekToken().Type != TokenType.Identifier) 
				{
					Error.ThrowError(Source.GetLine, Source.GetColumn, "[SRP2012] Expected identifier");
				}
				Expr subject = Expression(true);
				List<Expr> args = new List<Expr>();
				if (Source.PeekToken().Is(TokenType.LParen, "(")) 
				{
					Source.GetToken(); // (
					if (! Source.PeekToken().Is(TokenType.RParen, ")")) 
					{
						args.Add(Expression());
						while (Source.PeekToken().Is(TokenType.Comma, ",")) 
						{
							if (Source.PeekToken().Type == TokenType.EOF) {
								Error.ThrowError(Source.GetLine, Source.GetColumn, "[SRP2014] Expected [)] after argument list");
							}
							Source.GetToken(); // ,
							args.Add(Expression());
						}
					}
					Source.GetToken(); // )
				}
				Expr expr = new Expr(subject, args);
				expr.Type = ExprType.New;
				Operands.Push(expr);
			}
			else if (tok.Type == TokenType.LParen) 
			{
				Expr expr = Expression();
				if (Source.PeekToken().Type != TokenType.RParen) 
				{
					Error.ThrowError(Source.GetLine, Source.GetColumn, "[SRP2008] Expected [)] after expression");
				}
				Source.GetToken();
				Operands.Push(expr);
			}
			else 
			{
				Operands.Push(new Expr(tok));
			}
			while (Source.PeekToken().Type == TokenType.LBrace) 
			{
				Expr index = new Expr();
				index.Type = ExprType.Index;
				index.Subject = Operands.Pop();
				Source.GetToken(); // [
				index.Index = Expression();
				if (Source.PeekToken().Type != TokenType.RBrace) 
				{
					Error.ThrowError(Source.GetLine, Source.GetColumn, "[SRP2019] Expected []] after expression");
				}
				Source.GetToken(); // ]
				Operands.Push(index);
			}
			return Operands.Pop();
		}
		public Parser(string src) 
		{
			Source = new Lexer(src);
		}
	}
}