using System;
using System.Collections.Generic;
using Scrape;

namespace Scrape.Code.Generation {
    // Generate C++ code from a Scrape program.
    public class Compiler {
        private Parser Parser;

        private int IndentLevel = 0;

        public string Output = "";

        public ClassMember Entry;

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
            return member.Type + " " + member.Name + " = " + member.Expression.ToString() + ";\n";
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

            foreach (Stmt st in member.Body) {
                result += Indent();

                result += Statement(st);
            }

            IndentLevel--;

            return result + Indent() + "}\n\n";
        }

        public string Statement(Stmt st) {
            string result = "";

            if (st.Type == StmtType.Expression) {
                result += st.Expression.ToString() + ";\n";
            }

            if (st.Type == StmtType.If) {
                result += "if (" + st.Expression.ToString() + ") {\n";

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
                result += st.TypeName + " " + st.Name + " = " + st.Expression.ToString() + ";\n";
            }

            return result;
        }

        public string Class(TopLevel top) {
            string result = "";
            
            IndentLevel++;

            result += "class " + top.Name + " {\n" + Indent() + "public:\n\n";

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