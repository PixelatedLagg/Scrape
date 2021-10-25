using System;
using System.Collections.Generic;
using Scrape;

namespace Scrape.Code.Generation {

    public class TypeInfo {
        public string Name;

        public bool Static;

        public bool Method;
    }

    public enum ScopeType {
        Namespace,
        Class,
        Method
    }

    // Contain type declarations
    public class Scope {
        public ScopeType Type;

        public Scope Parent;

        public Dictionary<string, TypeInfo> Methods = new Dictionary<string, TypeInfo>();

        public Dictionary<string, TypeInfo> Variables = new Dictionary<string, TypeInfo>();

        public Dictionary<string, Scope> Classes = new Dictionary<string, Scope>();

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

        public void DefineClass(string name, Scope scope) {
            Classes[name] = scope;
        }

        public Scope GetClass(string name) {
            if (Classes.ContainsKey(name)) {
                return Classes[name];
            }

            if (Parent != null) {
                return Parent.GetClass(name);
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
    public class Compiler {
        private Parser Parser;

        private int IndentLevel = 0;

        public string Output = "";

        public ClassMember Entry;

        public Scope Context = new Scope(ScopeType.Namespace);

        public string Indent() {
            string result = "";

            for (int i = 0; i < IndentLevel; i++) {
                result += "\t";
            }

            return result;
        }

        public string Namespace(TopLevel top) {
            string result = "";

            result += "namespace " + top.Name + " {\n";

            IndentLevel++;

            foreach (TopLevel structure in top.NamespaceData) {
                result += Indent();

                if (structure.Type == TopLevelType.Namespace) {
                    result += Namespace(structure);
                }

                if (structure.Type == TopLevelType.Class) {
                    result += Class(structure);
                }
            }

            IndentLevel--;

            return result + Indent() + "}\n\n";
        }

        public string Field(ClassMember member) {
            Context.DefineVar(member.Name, new TypeInfo {
                Name = member.TypeName,

                Static = member.Modifiers.Contains("static")
            });

            return member.TypeName + " " + member.Name + " = " + member.Expression.ToString() + ";\n";
        }

        public string Method(ClassMember member) {
            string result = "";

            foreach (string mod in member.Modifiers) {
                if (mod == "static" && member.Name != "main") {
                    result += "static ";
                }
            }

            result += member.TypeName + " " + member.Name + $"() {{\n";

            IndentLevel++;

            Context.DefineMethod(member.Name, new TypeInfo {
                Name = member.TypeName,

                Static = member.Modifiers.Contains("static"),

                Method = true
            });

            Context = new Scope(Context, ScopeType.Method);

            foreach (Stmt st in member.Body) {
                result += Indent();

                result += Statement(st);
            }

            Context = Context.Parent;

            IndentLevel--;

            return result + Indent() + "}\n\n";
        }

        public string Statement(Stmt st) {
            string result = "";

            if (st.Type == StmtType.Expression) {
                result += Expression(st.Expression) + ";\n";
            }

            if (st.Type == StmtType.If) {
                result += "if (" + Expression(st.Expression) + ") {\n";

                IndentLevel++;

                foreach (Stmt stmt in st.Body) {
                    Indent();

                    Statement(stmt);
                }

                IndentLevel--;

                Indent();

                result += "}\n";
            }

            if (st.Type == StmtType.VarDef) {
                result += st.TypeName + " " + st.Name + " = " + Expression(st.Expression) + ";\n";
            }

            return result;
        }

        public string Class(TopLevel top) {
            string result = "";
            
            IndentLevel++;

            result += "class " + top.Name + " {\n" + Indent() + "public:\n\n";

            Context = new Scope(Context, ScopeType.Class);

            Context.Parent.DefineClass(top.Name, Context);

            foreach (ClassMember member in top.ClassData) {
                if (member.Name == "Main") {
                    member.Name = "main";
                    
                    Entry = member;

                    continue;
                }

                result += Indent();

                if (member.Type == ClassMemberType.Field) {
                    result += Field(member);
                }
                
                result += Method(member);
            }

            IndentLevel--;

            result += Indent() + "};\n\n";

            Context = Context.Parent;
            
            return result;
        }

        public string Expression(Expr expr) {
            string result = "";

            if (expr.Type == ExprType.Binary) {
                result += Expression(expr.Left);

                Scope cl = Context.GetClass(expr.Left.Value.String());

                // C++ uses :: instead of . for static members
                if (expr.Op.String() == "." && cl != null && cl.GetMethod(expr.Right.Value.String()).Static) {
                    result += "::";
                } else {
                    if (expr.Op.String() == ".") {
                        result += $"{expr.Op.String()}";
                    }
                    else {
                        result += $" {expr.Op.String()} ";
                    }
                }

                result += Expression(expr.Right);

                return result;
            }

            if (expr.Type == ExprType.Call) {
                result += Expression(expr.Subject) + "(";

                for (int i = 0; i < expr.Args.Count; i++) {
                    result += Expression(expr.Args[i]);

                    if (i < expr.Args.Count) {
                        result += ", ";
                    }
                }

                result += ")";

                return result;
            }

            if (expr.Type == ExprType.Literal) {
                result += expr.Value.String();
            }

            return result;
        }

        public void Compile() {
            TopLevel top = Parser.TopLevel();

            while (top != null) {
                if (top.Type == TopLevelType.Namespace) {
                    Output += Namespace(top);
                }

                if (top.Type == TopLevelType.Class) {
                    Output += Class(top);
                }

                top = Parser.TopLevel();
            }

            Output += Method(Entry);
        }

        public Compiler(string source) {
            Parser = new Parser(source);
        }
    }
}