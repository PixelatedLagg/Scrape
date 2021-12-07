using System;
using System.Collections.Generic;
using Scrape;

namespace Scrape.Code.Generation {

    public class TypeInfo {
        public string Name;

        public bool Static;

        public bool Namespace;

        public bool Method;

        public bool Class;

        public Scope Scope;
    }

    public enum ScopeType {
        Namespace,
        Class,
        Method
    }

    // Contain type declarations
    public class Scope {
        public string Name;

        public ScopeType Type;

        public Scope Parent;

        public Dictionary<string, TypeInfo> Namespaces = new Dictionary<string, TypeInfo>();

        public Dictionary<string, TypeInfo> Methods = new Dictionary<string, TypeInfo>();

        public Dictionary<string, TypeInfo> Variables = new Dictionary<string, TypeInfo>();

        public Dictionary<string, TypeInfo> Classes = new Dictionary<string, TypeInfo>();

        public void DefineNamespace(string name, TypeInfo info) {
            Namespaces[name] = info;
        }

        public TypeInfo GetNamespace(string name) {
            if (Namespaces.ContainsKey(name)) {
                return Namespaces[name];
            }

            if (Parent != null) {
                return Parent.GetNamespace(name);
            }
            
            return null;
        }

        public void DefineMethod(string name, TypeInfo info) {
            Methods[name] = info;
        }

        public TypeInfo GetMethod(string name) {
            if (Methods.ContainsKey(name)) {
                return Methods[name];
            }

            if (Parent != null) {
                return Parent.GetMethod(name);
            }

            return null;
        }

        public void DefineVar(string name, TypeInfo type) {
            Variables[name] = type;
        }

        public TypeInfo GetVar(string name) {
            if (Variables.ContainsKey(name)) {
                return Variables[name];
            }
            
            if (Parent != null) {
                return Parent.GetVar(name);
            }

            return null;
        }

        public void DefineClass(string name, TypeInfo info) {
            Classes[name] = info;
        }

        public TypeInfo GetClass(string name) {
            if (Classes.ContainsKey(name)) {
                return Classes[name];
            }

            if (Parent != null) {
                return Parent.GetClass(name);
            }

            return null;
        }

        public TypeInfo Get(string name) {
            if (Variables.ContainsKey(name)) {
                return Variables[name];
            }

            if (Methods.ContainsKey(name)) {
                return Methods[name];
            }

            if (Namespaces.ContainsKey(name)) {
                return Namespaces[name];
            }

            if (Classes.ContainsKey(name)) {
                return Classes[name];
            }

            if (Parent != null) {
                return Parent.Get(name);
            }

            return null;
        }

        public Scope(ScopeType type) {
            Type = type;
        }

        public Scope(Scope parent, ScopeType type) {
            Parent = parent;

            Type = type;
        }
    }

    // Generate C++ code from a Scrape program.
    public class Compiler 
    {
        private Parser Parser;
        private int IndentLevel = 0;
        public string Output = "";
        public Scope EntryContext;
        public ClassMember Entry;
        public Scope Context = new Scope(ScopeType.Namespace);
        public List<Scope> Used = new List<Scope>();
        public TypeInfo DeduceType(Expr expr) 
        {
            if (expr.Type == ExprType.Literal) 
            {
                string type = "error";
                switch (expr.Value.Type)
                {
                    case TokenType.String:
                        type = "string";
                        break;
                    case TokenType.Integer:
                        type = "int";
                        break;
                    case TokenType.Identifier:
                        return Context.Get(expr.Value.String());
                }
                if (expr.Value.String() == "true" || expr.Value.String() == "false") 
                {
                    type = "bool";
                }
                return new TypeInfo {Name = type};
            }
            switch (expr.Type)
            {
                case ExprType.New:
                    return new TypeInfo {Name = Expression(expr.Subject)};
                case ExprType.Call:
                    TypeInfo method = Context.GetMethod(expr.Value.String());
                    if (method == null) 
                    {
                        throw new Exception("Method not found: " + expr.Value.String());
                    }
                    return method;
            }
            return null;
        }
        private TypeInfo GetType(Expr typepath) 
        {
            Scope scope = Context.Get(typepath.Left?.Type == ExprType.Binary ? GetType(typepath.Left).Name : typepath.Left.Value.String())?.Scope;
            if (scope == null) 
            {
                throw new Exception("Type not found: " + typepath.Left.Value.String());
            }
            return scope.Get(typepath.Right.Value.String());
        }
        public string Indent() 
        {
            string result = "";
            for (int i = 0; i < IndentLevel; i++) 
            {
                result += "\t";
            }
            return result;
        }
        public string Namespace(TopLevel top) 
        {
            string result = "";
            result += "namespace " + top.Name + " {\n";
            IndentLevel++;
            Context = new Scope(Context, ScopeType.Namespace);
            Context.Name = top.Name;
            Context.Parent.DefineNamespace(top.Name, new TypeInfo {Name = top.Name, Static = true, Namespace = true, Scope = Context});
            foreach (TopLevel structure in top.NamespaceData) 
            {
                result += Indent();
                switch (structure.Type)
                {
                    case TopLevelType.Namespace:
                        result += Namespace(structure);
                        break;
                    case TopLevelType.Class:
                        result += Class(structure);
                        break;
                }
            }
            Context = Context.Parent;
            IndentLevel--;
            return $"{result}{Indent()}}}\n\n";
        }
        public string Field(ClassMember member) 
        {
            Context.DefineVar(member.Name, new TypeInfo {Name = member.TypeName, Static = member.Modifiers.Contains("static")});
	        return $"{member.TypeName} {member.Name} = {member.Expression.ToString()};\n";
        }
        public string Method(ClassMember member) 
        {
            string result = "";
            if (member.Modifiers.Contains("extern") || member.Modifiers.Contains("abstract")) 
            {
                Context.DefineMethod(member.Name, new TypeInfo {Name = member.TypeName,Static = member.Modifiers.Contains("static"),Method = true});
                return "";
            }
            foreach (string mod in member.Modifiers) 
            {
                if (mod == "static" && member.Name != "main") 
                {
                    result += "static ";
                }
            }
            result += $"{member.TypeName} {member.Name}(";
            for (int i = 0; i < member.ArgNames.Count; i++) 
            {
                result += member.ArgTypes[i] + " " + member.ArgNames[i];
                if (i < member.ArgNames.Count - 1) 
                {
                    result += ", ";
                }
            }
            result += ") {\n";
            IndentLevel++;
            Context.DefineMethod(member.Name, new TypeInfo {Name = member.TypeName,Static = member.Modifiers.Contains("static"),Method = true});
            Context = new Scope(Context, ScopeType.Method);
            foreach (Stmt st in member.Body) 
            {
                result += Indent();
                result += Statement(st);
            }
            Context = Context.Parent;
            IndentLevel--;
            return $"{result}{Indent()}}}\n\n";
        }
        public string Statement(Stmt st) 
        {
            string result = "";
            switch (st.Type)
            {
                case StmtType.Expression:
                    result += $"{Expression(st.Expression)};\n";
                    break;
                case StmtType.Return:
                    result += $"return {Expression(st.Expression)};\n";
                    break;
                case StmtType.Assignment:
                    result += $"if ({Expression(st.Path)} != nullptr) {Expression(st.Path)}->S_Handle->Unref();\n{Expression(st.Path)} = {Expression(st.Expression)};\n";
                    break;
                case StmtType.If:
                    result += $"if ({Expression(st.Condition)}) {{\n";
                    IndentLevel++;
                    foreach (Stmt stmt in st.Body) 
                    {
                        result += $"{Indent()}{Statement(stmt)}";
                    }
                    IndentLevel--;
                    result += $"{Indent()}}}\n\n";
                    break;
                case StmtType.VarDef:
                    TypeInfo type = Context.GetClass(st.TypeName);
                    if (type == null) 
                    {
                        foreach (Scope ns in Used) 
                        {
                            type = ns.GetClass(st.TypeName);
                            if (type != null) 
                            {
                                break;
                            }
                        }
                    }
                    string tname = st.TypeName;
                    if (tname != DeduceType(st.Expression).Name) 
                    {
                        throw new Exception($"Type mismatch in variable definition (assigning '{DeduceType(st.Expression).Name}' to '{tname}')");
                    }
                    if (type != null) 
                    {
                        tname = $"{type.Name}*";
                    }
                    result += $"{tname} {st.Name} = {Expression(st.Expression)};\n";
                    break;
            }
            return result;
        } //will pick up code reformatting tomorrow
        public string Class(TopLevel top) {
            string result = "";
            
            IndentLevel++;

            result += "class " + top.Name + " : S_Object {\n" + Indent() + "public:\n\n";

            Context = new Scope(Context, ScopeType.Class);

            Context.Name = $"{Context.Parent.Name}::{top.Name}";

            Context.Parent.DefineClass(top.Name, new TypeInfo {
                Name = top.Name,

                Static = true,

                Class = true,

                Scope = Context
            });

            bool emit = false;

            foreach (ClassMember member in top.ClassData) {
                if (member.Name == "Main" && Global.EntryPoint)
                {
                    Error.ThrowError("More than one entrypoint specified!");
                }
                if (member.Name == "Main") {
                    Global.EntryPoint = true;
                    member.Name = "main";
                    
                    EntryContext = Context;
                    Entry = member;

                    continue;
                }
                if (! member.Modifiers.Contains("abstract") && ! member.Modifiers.Contains("extern")) {
                    emit = true;
                }

                result += Indent();

                if (member.Type == ClassMemberType.Field) {
                    result += Field(member);

                    continue;
                }
                
                result += Method(member);
            }

            IndentLevel--;

            result += Indent() + "};\n\n";

            Context = Context.Parent;
            
            return emit ? result : "";
        }

        public string Expression(Expr expr) {
            string result = "";

            if (expr.Type == ExprType.New) {
                // result += "new " + Expression(expr.Subject) + "(";

                result += $"S_GC::Alloc<{Expression(expr.Subject)}>()";

                for (int i = 0; i < expr.Args.Count; i++) {
                    result += Expression(expr.Args[i]);

                    if (i < expr.Args.Count - 1) {
                        result += ", ";
                    }
                }

                result += ")";
            }

            if (expr.Type == ExprType.Binary) {
                Scope cl = Context.Get(expr.Left.Value.String())?.Scope;

                if (cl == null) {
                    foreach (Scope ns in Used) {
                        cl = ns.Get(expr.Left.Value.String())?.Scope;

                        if (cl != null) {
                            break;
                        }
                    }
                }

                TypeInfo righttype = cl?.Get(expr.Right.Value.String());

                // C++ uses :: instead of . for static members
                if (expr.Op.String() == "." && cl != null && righttype != null && righttype.Static) {
                    result += Expression(expr.Left);

                    result += "::";
                } else {
                    if (expr.Op.String() == ".") {
                        result += "(*" + Expression(expr.Left) + ")[\"";
                    }
                    else {
                        result += Expression(expr.Left);

                        result += $" {expr.Op.String()} ";
                    }
                }

                result += Expression(expr.Right);

                if (expr.Op.String() == "." && righttype?.Static == null || righttype?.Static == false) {
                    result += "\"]";
                }

                return result;
            }

            if (expr.Type == ExprType.Call) {
                result += Expression(expr.Subject) + "(";

                for (int i = 0; i < expr.Args.Count; i++) {
                    result += Expression(expr.Args[i]);

                    if (i < expr.Args.Count - 1) {
                        result += ", ";
                    }
                }

                result += ")";

                return result;
            }

			if (expr.Type == ExprType.Index) {
				return $"(*{Expression(expr.Subject)})[{Expression(expr.Index)}]";
			}

            if (expr.Type == ExprType.Literal) {
                if (expr.Value.Type == TokenType.String) {
                    result += "\"" + expr.Value.String() + "\"";

                    return result;
                }

                result += expr.Value.String();
            }

            return result;
        }

        public void Compile() {
            TopLevel top = Parser.TopLevel();

            //Output += "#include <scrape.hpp>\n\n";

            while (top != null) {
                if (top.Type == TopLevelType.Namespace) {
                    Output += Namespace(top);
                }

                if (top.Type == TopLevelType.Class) {
                    Output += Class(top);
                }

                if (top.Type == TopLevelType.Using) {
                    string path = Expression(top.Path);

                    if (Context.GetNamespace(path) != null) {
                        Used.Add(Context.GetNamespace(path).Scope);
                    }

                    Output += "using namespace " + path + ";\n\n";
                }

                top = Parser.TopLevel();
            }

			if (EntryContext != null) {
				Output += $"using namespace {EntryContext.Parent.Name};\n\n";

				Context = EntryContext;

				Output += Method(Entry);
			}
        }

        public Compiler(string source) {
            Parser = new Parser(source);
        }
    }
}
