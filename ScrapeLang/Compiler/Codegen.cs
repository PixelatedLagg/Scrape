using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using Scrape;
using LLVMSharp;

namespace Scrape.Code.Generation {

    public class TypeInfo {
        public string Name;

        public bool Static;

        public bool Namespace;

        public bool Method;

        public bool Class;

		public bool Argument;

        public Scope Scope;

		public Expr InitialValue;

		public int FieldIndex;

		public LLVMTypeRef LLVMType;

		public LLVMValueRef LLVMValue;
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

		public LLVMContextRef LLVMContext;

        public Dictionary<string, TypeInfo> Namespaces = new Dictionary<string, TypeInfo>();

        public Dictionary<string, TypeInfo> Methods = new Dictionary<string, TypeInfo>();

        public Dictionary<string, TypeInfo> Variables = new Dictionary<string, TypeInfo>();

        public Dictionary<string, TypeInfo> Classes = new Dictionary<string, TypeInfo>();

		public Dictionary<string, LLVMTypeRef> LLVMTypes = new Dictionary<string, LLVMTypeRef>();

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

			LLVMContext = LLVM.ContextCreate();
        }

        public Scope(Scope parent, ScopeType type) {
            Parent = parent;

            Type = type;

			LLVMContext = LLVM.ContextCreate();
        }
    }

	// Use LLVM to generate code
	public class LLVMCompiler {
		private Parser Parser;

		public Scope EntryContext;

        public ClassMember Entry;

        public Scope Context = new Scope(ScopeType.Namespace);

		public LLVMModuleRef Module;

		public LLVMValueRef Function;

		public Dictionary<string, LLVMModuleRef> Namespaces = new Dictionary<string, LLVMModuleRef>();

		public List<Scope> Used = new List<Scope>();

		public TypeInfo DeduceType(Expr expr) {
			if (expr.Type == ExprType.Literal) {
				string type = "error";

				if (expr.Value.Type == TokenType.String) {
					type = "string";
				} else if (expr.Value.Type == TokenType.Integer) {
					type = "int";
				} else if (expr.Value.String() == "true" || expr.Value.String() == "false") {
					type = "bool";
				}

				if (expr.Value.Type == TokenType.Identifier) {
					return Context.Get(expr.Value.String());
				}

				return new TypeInfo {
					Name = type
				};
			}

			if (expr.Type == ExprType.New) {
				return new TypeInfo {
					Name = expr.Subject.Value.String()
				};
			}

			if (expr.Type == ExprType.Call) {
				TypeInfo method = Context.GetMethod(expr.Value.String());

				if (method == null) {
					throw new Exception("Method not found: " + expr.Value.String());
				}

				return method;
			}

			if (expr.Type == ExprType.Binary) {
				if (expr.Op.String() == ".") {
					TypeInfo left = DeduceType(expr.Left);

					if (left == null) {
						throw new Exception("Left side of dot operator is not a type");
					}

					Console.WriteLine(left.Scope);

					TypeInfo right = left.Scope.Get(expr.Right.Value.String());

					return right;
				}
			}

			return null;
		}

		private TypeInfo GetType(Expr typepath) {
			//TypeInfo result = null;

			if (typepath.Value.String() == "int") {
				return new TypeInfo {
					Name = "int"
				};
			}

			if (typepath.Value.String() == "string") {
				return new TypeInfo {
					Name = "string"
				};
			}

			if (typepath.Left == null) {
				return Context.Get(typepath.Value.String());
			}

			Scope scope = Context.Get(typepath.Left?.Type == ExprType.Binary ? GetType(typepath.Left).Name : typepath.Left.Value.String())?.Scope;

			if (scope == null) {
				throw new Exception("Type not found: " + typepath.ToString());
			}

			return scope.Get(typepath.Right.Value.String());
		}

		public LLVMModuleRef Namespace(TopLevel top) {
            LLVMModuleRef prev = Module;

            Context = new Scope(Context, ScopeType.Namespace);

            Context.Name = top.Name;

			Module = LLVM.ModuleCreateWithName(top.Name);

            Context.Parent.DefineNamespace(top.Name, new TypeInfo {
                Name = top.Name,

                Static = true,

                Namespace = true,

                Scope = Context
            });

            foreach (TopLevel structure in top.NamespaceData) {

                if (structure.Type == TopLevelType.Namespace) {
                    Namespace(structure);
                }

                if (structure.Type == TopLevelType.Class) {
                    Class(structure);
                }
            }

			LLVMModuleRef mod = Module;

            Context = Context.Parent;

			mod = prev;

			return Module;
        }

		public void Class(TopLevel top) {
            Context = new Scope(Context, ScopeType.Class);

            Context.Name = top.Name;

			// LLVM Class Generation

			List<LLVMTypeRef> fields = new List<LLVMTypeRef>();

            foreach (ClassMember member in top.ClassData) {
                if (member.Name == "Main" && Global.Entrypoint)
                {
                    throw new CompileError("More than one entrypoint provided!");
                }

                if (! member.Modifiers.Contains("abstract") && ! member.Modifiers.Contains("extern")) {
                    // emit = true;
                }

                if (member.Type == ClassMemberType.Field) {
                    fields.Add(Field(member));

                    continue;
                }
            }

			LLVMTypeRef[] fieldTypes = fields.ToArray();

			LLVMTypeRef classType = LLVM.StructType(fieldTypes, false);

			Context.LLVMTypes.Add(top.Name, classType);

			Context.Parent.DefineClass(top.Name, new TypeInfo {
                Name = top.Name,

                Static = true,

                Class = true,

                Scope = Context,

				LLVMType = classType
            });

			foreach (ClassMember member in top.ClassData) {
                if (member.Type == ClassMemberType.Field)
                    continue;
                
            	Method(member, classType);
            }

            Context = Context.Parent;
        }

		private LLVMTypeRef TypeNameToLLVMType(string typename) {
			if (typename == "int") {
				return LLVM.Int32Type();
			}

			if (typename == "char") {
				return LLVM.Int8Type();
			}

			if (typename == "string") {
				return LLVM.PointerType(LLVM.Int8Type(), 0);
			}

			if (typename == "bool") {
				return LLVM.Int1Type();
			}

			if (typename == "void") {
				return LLVM.VoidType();
			}

			TypeInfo type = Context.GetClass(typename);

			if (type != null)
				return type.LLVMType;

			return LLVM.PointerType(LLVM.Int8Type(), 0);
		}

		private LLVMTypeRef TypeExprToLLVMType(Expr typeexpr) {
			if (typeexpr.Type == ExprType.Array) {
				if (typeexpr.ArrayLengths[0] == 0) {
					LLVMTypeRef ptr = LLVM.PointerType(TypeNameToLLVMType(typeexpr.Subject.Value.String()), 0);

					for (int i = 1; i < typeexpr.ArrayDepth; i++) {
						ptr = LLVM.PointerType(ptr, 0);
					}

					return ptr;
				}
				
				LLVMTypeRef array = LLVM.ArrayType(TypeNameToLLVMType(typeexpr.Subject.Value.String()), (uint) typeexpr.ArrayLengths[0]);

				for (int i = 1; i < typeexpr.ArrayDepth; i++) {
					array = LLVM.ArrayType(array, (uint) typeexpr.ArrayLengths[i]);
				}

				return array;
			}
			
			return TypeNameToLLVMType(typeexpr.Subject.Value.String());
		}

		public LLVMTypeRef Field(ClassMember member) {
            Context.DefineVar(member.Name, new TypeInfo {
                Name = member.TypeName,

				FieldIndex = Context.Variables.Count,

				InitialValue = member.Expression,

                Static = member.Modifiers.Contains("static")
            });

            LLVMTypeRef type = TypeNameToLLVMType(member.TypeName);

			if (Context.GetClass(member.TypeName) != null)
				return LLVM.PointerType(type, 0);
			
			return type;
        }

		// Adds function to current module
		public void Method(ClassMember member, LLVMTypeRef classtype) {
			List<LLVMTypeRef> args = new List<LLVMTypeRef>();

			if (! member.Modifiers.Contains("static")) {
				args.Add(LLVM.PointerType(classtype, 0));
			}

            foreach (Expr arg in member.ArgTypes) {
				args.Add(TypeExprToLLVMType(arg));
			}

			LLVMTypeRef fntype = LLVM.FunctionType(TypeNameToLLVMType(member.TypeName), args.ToArray(), false);

			LLVMValueRef fn = LLVM.AddFunction(Module, member.Name, fntype);

			LLVMValueRef prev = Function;

			Function = fn;

            Context.DefineMethod(member.Name, new TypeInfo {
                Name = member.TypeName,

                Static = member.Modifiers.Contains("static"),

                Method = true,

				LLVMValue = fn
            });

            Context = new Scope(Context, ScopeType.Method);

			if (! member.Modifiers.Contains("static")) {
				Context.DefineClass("this", new TypeInfo {
					Name = Context.Parent.Name,

					Static = member.Modifiers.Contains("static"),

					Class = true,

					Scope = Context.Parent
				});
			}

			for (int i = 0; i < member.ArgNames.Count; i++) {
                Context.DefineVar(member.ArgNames[i], new TypeInfo {
					Name = member.ArgTypes[i].Subject.Value.String(),

					Argument = true,

					LLVMValue = LLVM.GetParam(fn, (uint) (i + (! member.Modifiers.Contains("static") ? 1 : 0)))
				});
            }

			if (member.Modifiers.Contains("extern")) {
				Context = Context.Parent;

				Function = prev;
				
				fn.SetLinkage(LLVMLinkage.LLVMExternalLinkage);
				
				return;
			}

			LLVMBuilderRef builder = LLVM.CreateBuilderInContext(Context.LLVMContext);

			LLVMBasicBlockRef entry = LLVM.AppendBasicBlock(fn, "entry");

			LLVM.PositionBuilderAtEnd(builder, entry);

            foreach (Stmt st in member.Body) {
                Statement(builder, st);
            }

			Console.WriteLine("Added fn: " + member.Name);

			// LLVM.DisposeBuilder(builder);

            Context = Context.Parent;

			Function = prev;
        }

		public void Statement(LLVMBuilderRef builder, Stmt st) {
            if (st.Type == StmtType.Expression) {
                Expression(builder, st.Expression);
            }

            if (st.Type == StmtType.Return) {
				Console.WriteLine("Return");
				
               LLVM.BuildRet(builder, Expression(builder, st.Expression));
            }

            if (st.Type == StmtType.Assignment) {
				TypeInfo tinfo = GetType(st.Path);

                LLVMValueRef subject = tinfo.LLVMValue;

				LLVM.BuildStore(builder, Expression(builder, st.Expression), subject);
            }

            if (st.Type == StmtType.If) {
				LLVMValueRef cond = Expression(builder, st.Condition);

				LLVMBasicBlockRef iftrue = LLVM.AppendBasicBlock(Function, "iftrue");

				LLVMBuilderRef truebuilder = LLVM.CreateBuilderInContext(Context.LLVMContext);

				LLVM.PositionBuilderAtEnd(truebuilder, iftrue);

				foreach (Stmt stmt in st.Body) {
                    Statement(truebuilder, stmt);
                }

				LLVMBasicBlockRef iffalse = LLVM.AppendBasicBlock(Function, "iffalse");

				LLVMBuilderRef falsebuilder = LLVM.CreateBuilderInContext(Context.LLVMContext);

				LLVM.PositionBuilderAtEnd(falsebuilder, iffalse);

				LLVM.BuildBr(truebuilder, iffalse);

				LLVM.BuildCondBr(builder, cond, iftrue, iffalse);

				LLVM.PositionBuilderAtEnd(builder, iffalse);
            }

			if (st.Type == StmtType.While) {
				LLVMBasicBlockRef block = LLVM.AppendBasicBlock(Function, "while");

				LLVMBuilderRef builder2 = LLVM.CreateBuilderInContext(Context.LLVMContext);

				LLVM.PositionBuilderAtEnd(builder2, block);

				foreach (Stmt stmt in st.Body) {
					Statement(builder2, stmt);
				}

				LLVMBasicBlockRef end = LLVM.AppendBasicBlock(Function, "end");

				LLVM.BuildCondBr(builder, Expression(builder, st.Condition), block, end);

				LLVM.BuildCondBr(builder2, Expression(builder2, st.Condition), block, end);

				LLVM.PositionBuilderAtEnd(builder, end);
			}

            if (st.Type == StmtType.VarDef) {
                TypeInfo type = Context.GetClass(st.TypeName);

                if (type == null) {
                    foreach (Scope ns in Used) {
                        type = ns.GetClass(st.TypeName);

                        if (type != null) {
                            break;
                        }
                    }
                }

                string tname = st.TypeName;

                if (tname != DeduceType(st.Expression).Name) {
                    throw new Exception($"Type mismatch in variable definition (assigning '{DeduceType(st.Expression).Name}' to '{tname}')");
                }

				LLVMTypeRef typeref = Context.GetClass(tname) != null ? LLVM.PointerType(TypeExprToLLVMType(st.TypeExpr), 0) : TypeExprToLLVMType(st.TypeExpr);

				LLVMValueRef stack = LLVM.BuildAlloca(builder, typeref, "stackalloc");

                Context.DefineVar(st.Name, new TypeInfo {
					Name = tname,

					Static = st.Modifiers.Contains("static"),

					Scope = type?.Scope,

					LLVMValue = stack
				});

				LLVM.BuildStore(builder, Expression(builder, st.Expression), stack);
			}
        }

		LLVMValueRef TempObject;

		public LLVMValueRef Expression(LLVMBuilderRef builder, Expr expr) {
            if (expr.Type == ExprType.New) {
				TypeInfo type = Context.GetClass(expr.Subject.Value.String());

                LLVMValueRef ptr = LLVM.BuildAlloca(builder, TypeNameToLLVMType(expr.Subject.Value.String()), "stackobj");

				foreach (KeyValuePair<string, TypeInfo> field in type.Scope.Variables) {
					if (field.Value.InitialValue != null) {
						/*LLVMValueRef target = LLVM.BuildGEP(builder, ptr, new LLVMValueRef[] {
							LLVM.ConstInt(LLVM.Int32Type(), 0, false)
						}, "field_ptr");*/

						LLVMValueRef target = LLVM.BuildStructGEP(builder, ptr, (uint) field.Value.FieldIndex, field.Key);

						Console.WriteLine($"${field.Value.Name}: ${Context.GetClass(field.Value.Name) != null}");

						if (Context.GetClass(field.Value.Name) != null) {
							Console.WriteLine("Class");
							
							LLVM.BuildStore(builder, Expression(builder, field.Value.InitialValue), target);

							continue;
						}

						target = LLVM.BuildGEP(builder, target, new LLVMValueRef[] {
							LLVM.ConstInt(LLVM.Int32Type(), 0, false)
						}, "field_ptr");

						LLVM.BuildStore(builder, Expression(builder, field.Value.InitialValue), target);
					}
				}

				return ptr;
            }

            if (expr.Type == ExprType.Binary) {
                if (expr.Op.String() == "+") {
					return LLVM.BuildAdd(builder, Expression(builder, expr.Left), Expression(builder, expr.Right), "add");
				}

				if (expr.Op.String() == "-") {
					return LLVM.BuildSub(builder, Expression(builder, expr.Left), Expression(builder, expr.Right), "sub");
				}

				if (expr.Op.String() == "*") {
					return LLVM.BuildMul(builder, Expression(builder, expr.Left), Expression(builder, expr.Right), "mul");
				}

				if (expr.Op.String() == "/") {
					return LLVM.BuildUDiv(builder, Expression(builder, expr.Left), Expression(builder, expr.Right), "div");
				}

				if (expr.Op.String() == "==") {
					return LLVM.BuildICmp(builder, LLVMIntPredicate.LLVMIntEQ, Expression(builder, expr.Left), Expression(builder, expr.Right), "eq");
				}

				if (expr.Op.String() == ".") {
					TypeInfo tinfo = DeduceType(expr.Left);

					tinfo = Context.GetClass(tinfo.Name);

					TypeInfo right = tinfo.Scope.Get(expr.Right.Value.String());

					if (right.Method) {
						TempObject = Expression(builder, expr.Left);

						return LLVM.GetNamedFunction(Module, expr.Right.Value.String());
					}

					LLVMValueRef ptr = Expression(builder, expr.Left);

					ptr.Dump();

					return LLVM.BuildLoad(builder, LLVM.BuildStructGEP(builder, ptr, (uint) right.FieldIndex, right.Name), "field");
				}
            }

            if (expr.Type == ExprType.Call) {
				List<LLVMValueRef> args = new List<LLVMValueRef>();

				TypeInfo type = DeduceType(expr.Subject);

				LLVMValueRef fn = Expression(builder, expr.Subject);

				if (! type.Static) {
					if (TempObject.Pointer != IntPtr.Zero)
						args.Add(TempObject);
					else
						args.Add(LLVM.GetParam(Function, 0));
				}

				TempObject = new LLVMValueRef(IntPtr.Zero);

				foreach (Expr arg in expr.Args) {
					args.Add(Expression(builder, arg));
				}

            	return LLVM.BuildCall(builder, fn, args.ToArray(), "");
            }

			if (expr.Type == ExprType.Index) {
				LLVMValueRef ptr = Expression(builder, expr.Subject);

				LLVMValueRef index = Expression(builder, expr.Index);

				return LLVM.BuildLoad(builder, LLVM.BuildGEP(builder, ptr, new LLVMValueRef[] {
					index
				}, "index_gep"), "index_load");
			}

            if (expr.Type == ExprType.Literal) {
				if (expr.Value.Type == TokenType.Identifier) {
					if (expr.Value.String() == "this") {
						return LLVM.GetParam(Function, 0);
					}

					if (expr.Value.String() == "true") {
						return LLVM.ConstInt(LLVM.Int1Type(), 1, false);
					}

					if (expr.Value.String() == "false") {
						return LLVM.ConstInt(LLVM.Int1Type(), 0, false);
					}

					TypeInfo tinfo = Context.Get(expr.Value.String());

					if (tinfo != null && tinfo.Method) {
						return LLVM.GetNamedFunction(Module, expr.Value.String());
					}

					if (tinfo != null && tinfo.Argument)
						return tinfo.LLVMValue;

					return LLVM.BuildLoad(builder, tinfo.LLVMValue, "load");
				}

				if (expr.Value.Type == TokenType.String) {
					return LLVM.BuildGlobalStringPtr(builder, expr.Value.String(), "str");
				}

                if (expr.Value.Type == TokenType.Integer) {
					return LLVM.ConstInt(LLVM.Int32Type(), (ulong) expr.Value.Integer(), false);
				}
            }

			return LLVM.ConstNull(LLVM.Int32Type());
        }

		private delegate int MainDelegate();

		public void Compile() {
            TopLevel top = Parser.TopLevel();

			LLVM.InitializeX86Target();
			LLVM.InitializeX86AsmParser();
			LLVM.InitializeX86AsmPrinter();
			LLVM.InitializeX86TargetInfo();
			LLVM.InitializeX86TargetMC();

            while (top != null) {
                if (top.Type == TopLevelType.Namespace) {
                    Namespace(top);
                }

                if (top.Type == TopLevelType.Class) {
                    Class(top);
                }

                if (top.Type == TopLevelType.Using) {
                    // Used.Add(Context.GetNamespace(top.Path.Value.String()).Scope);
                }

                top = Parser.TopLevel();
            }

			string msg;

			LLVM.VerifyModule(Module, LLVMVerifierFailureAction.LLVMPrintMessageAction, out msg);

			Console.WriteLine("LLVM Message: " + msg);

			// Console.WriteLine(LLVM.CreateMCJITCompilerForModule(out engine, Module, new LLVMMCJITCompilerOptions(), out msg));

			/*LLVM.CreateExecutionEngineForModule(out engine, Module, out msg);

			LLVMGenericValueRef val = LLVM.RunFunction(engine, LLVM.GetNamedFunction(Module, "Main"), new LLVMGenericValueRef[] { });

			// Get int from genericvalueref
			int ret = (int) LLVM.GenericValueToInt(val, true);

			Console.WriteLine("Returned: " + ret);*/

			LLVM.SetTarget(Module, "x86_64-unknown-linux-elf");

			LLVMTargetRef target;

			LLVM.GetTargetFromTriple("x86_64-unknown-linux-elf", out target, out msg);

			Console.WriteLine(msg);

			LLVMTargetMachineRef machine = LLVM.CreateTargetMachine(target, "x86_64-unknown-linux-elf", "generic", "", LLVMCodeGenOptLevel.LLVMCodeGenLevelDefault, LLVMRelocMode.LLVMRelocDefault, LLVMCodeModel.LLVMCodeModelDefault);

			LLVM.TargetMachineEmitToFile(machine, Module, Marshal.StringToHGlobalAnsi("object.o"), LLVMCodeGenFileType.LLVMObjectFile, out msg);

			LLVM.DisposeTargetMachine(machine);



			if (EntryContext != null) {
				Context = EntryContext;

				// Method(Entry);
			}
        }

		public LLVMCompiler(string source) {
			Parser = new Parser(source);

			Module = LLVM.ModuleCreateWithName("_Main");

			/*Context.DefineClass("int", new TypeInfo {
				Name = "int",

				Argument = false,

				LLVMType = LLVM.Int32Type()
			});*/
		}
	}

    // Generate C++ code from a Scrape program.
    public class Compiler {
        private Parser Parser;

        private int IndentLevel = 0;

        public string Output = "";

        public Scope EntryContext;

        public ClassMember Entry;

        public Scope Context = new Scope(ScopeType.Namespace);

        public List<Scope> Used = new List<Scope>();

        public TypeInfo DeduceType(Expr expr) {
            if (expr.Type == ExprType.Literal) {
                string type = "error";

                if (expr.Value.Type == TokenType.String) {
                    type = "string";
                } else if (expr.Value.Type == TokenType.Integer) {
                    type = "int";
                } else if (expr.Value.String() == "true" || expr.Value.String() == "false") {
                    type = "bool";
                }

                if (expr.Value.Type == TokenType.Identifier) {
                    return Context.Get(expr.Value.String());
                }

                return new TypeInfo {
                    Name = type
                };
            }

            if (expr.Type == ExprType.New) {
                return new TypeInfo {
                    Name = Expression(expr.Subject)
                };
            }

            if (expr.Type == ExprType.Call) {
                TypeInfo method = Context.GetMethod(expr.Value.String());

                if (method == null) {
                    throw new Exception("Method not found: " + expr.Value.String());
                }

                return method;
            }

            return null;
        }

        private TypeInfo GetType(Expr typepath) {
            //TypeInfo result = null;

            Scope scope = Context.Get(typepath.Left?.Type == ExprType.Binary ? GetType(typepath.Left).Name : typepath.Left.Value.String())?.Scope;

            if (scope == null) {
                throw new Exception("Type not found: " + typepath.Left.Value.String());
            }

            return scope.Get(typepath.Right.Value.String());
        }

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

            Context = new Scope(Context, ScopeType.Namespace);

            Context.Name = top.Name;

            Context.Parent.DefineNamespace(top.Name, new TypeInfo {
                Name = top.Name,

                Static = true,

                Namespace = true,

                Scope = Context
            });

            foreach (TopLevel structure in top.NamespaceData) {
                result += Indent();

                if (structure.Type == TopLevelType.Namespace) {
                    result += Namespace(structure);
                }

                if (structure.Type == TopLevelType.Class) {
                    result += Class(structure);
                }
            }

            Context = Context.Parent;

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

            if (member.Modifiers.Contains("extern") || member.Modifiers.Contains("abstract")) {
                Context.DefineMethod(member.Name, new TypeInfo {
                    Name = member.TypeName,

                    Static = member.Modifiers.Contains("static"),

                    Method = true
                });

                return "";
            }

            foreach (string mod in member.Modifiers) {
                if (mod == "static" && member.Name != "main") {
                    result += "static ";
                }
            }

            result += member.TypeName + " " + member.Name + "(";

            for (int i = 0; i < member.ArgNames.Count; i++) {
                result += member.ArgTypes[i] + " " + member.ArgNames[i];

                if (i < member.ArgNames.Count - 1) {
                    result += ", ";
                }
            }
                
            result += ") {\n";

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

            if (st.Type == StmtType.Return) {
                result += "return " + Expression(st.Expression) + ";\n";
            }

            if (st.Type == StmtType.Assignment) {
                result += $"if ({Expression(st.Path)} != nullptr) {Expression(st.Path)}->S_Handle->Unref();\n";

                result += Expression(st.Path) + " = " + Expression(st.Expression) + ";\n";
            }

            if (st.Type == StmtType.If) {
                result += "if (" + Expression(st.Condition) + ") {\n";

                IndentLevel++;

                foreach (Stmt stmt in st.Body) {
                    result += Indent();

                    result += Statement(stmt);
                }

                IndentLevel--;

                result += Indent();

                result += "}\n\n";
            }

            if (st.Type == StmtType.VarDef) {
                TypeInfo type = Context.GetClass(st.TypeName);

                if (type == null) {
                    foreach (Scope ns in Used) {
                        type = ns.GetClass(st.TypeName);

                        if (type != null) {
                            break;
                        }
                    }
                }

                string tname = st.TypeName;

                if (tname != DeduceType(st.Expression).Name) {
                    throw new Exception($"Type mismatch in variable definition (assigning '{DeduceType(st.Expression).Name}' to '{tname}')");
                }

                if (type != null) {
                    tname = type.Name + '*';
                }

                result += tname + " " + st.Name + " = " + Expression(st.Expression) + ";\n";
            }

            return result;
        }

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
                if (member.Name == "Main" && Global.Entrypoint)
                {
                    throw new CompileError("More than one entrypoint provided!");
                }
                if (member.Name == "Main") {
                    Global.Entrypoint = true;
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

            Output += "#include <scrape.hpp>\n\n";

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