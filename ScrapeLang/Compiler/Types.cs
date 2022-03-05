using System;
using System.Collections.Generic;
using Scrape;
using LLVMSharp;

namespace Scrape.Code.Generation {
	public class StructHandle {
		private LLVMBuilderRef Builder;

		public LLVMValueRef Value;

		public void SetField(uint index, LLVMValueRef value) {
			LLVM.BuildStore(Builder, value, LLVM.BuildStructGEP(Builder, Value, index, "set_field"));
		}

		public StructHandle(LLVMBuilderRef builder, LLVMValueRef value) {
			Builder = builder;

			Value = value;
		}
	}

	public class StructBuilder {
		private LLVMModuleRef Module;

		public string Name;

		public bool Packed = false;

		public List<LLVMTypeRef> Fields = new List<LLVMTypeRef>();

		public List<LLVMValueRef> FieldValues = new List<LLVMValueRef>();

		public List<LLVMValueRef> HeapReferences = new List<LLVMValueRef>();

		public void AddField(LLVMTypeRef type) {
			Fields.Add(type);
			
			FieldValues.Add(LLVM.ConstNull(type));
		}

		public void AddField(LLVMTypeRef type, LLVMValueRef value) {
			Fields.Add(type);
			
			FieldValues.Add(value);

			LLVMTypeRef strct = type;

			// while?
			if (strct.TypeKind == LLVMTypeKind.LLVMPointerTypeKind)
				strct = strct.GetElementType();

			if (strct.TypeKind == LLVMTypeKind.LLVMStructTypeKind) {
				HeapReferences.Add(value);
			}
		}

		public LLVMTypeRef GetLLVMType() {
			LLVMTypeRef[] types = new LLVMTypeRef[Fields.Count];

			// types[types.Length - 1] = LLVM.ArrayType(LLVM.Int64Type(), (uint) HeapReferences.Count);
			
			for (int i = 0; i < Fields.Count; i++) {
				types[i] = Fields[i];
			}

			return LLVM.StructType(types, Packed);
		}

		public StructHandle Construct(LLVMBuilderRef builder) {
			LLVMTypeRef[] types = new LLVMTypeRef[Fields.Count];

			// types[types.Length - 1] = LLVM.ArrayType(LLVM.Int64Type(), (uint) HeapReferences.Count);
			
			for (int x = 0; x < Fields.Count; x++) {
				types[x] = Fields[x];
			}
			
			LLVMValueRef value = LLVM.BuildAlloca(builder, LLVM.StructType(types, Packed), "struct_" + Name);

			for (uint i = 0; i < FieldValues.Count; i++) {
				LLVM.BuildStore(builder, FieldValues[(int) i], LLVM.BuildStructGEP(builder, value, i, "set_field_" + i));
			}

			/*LLVMValueRef heaprefs = LLVM.BuildStructGEP(builder, value, (uint) FieldValues.Count, "heap_refs");

			for (uint i = 0; i < HeapReferences.Count; i++) {
				LLVM.BuildStore(builder, HeapReferences[(int) i], LLVM.BuildGEP(builder, heaprefs, new LLVMValueRef[] { LLVM.BuildBitCast(builder, LLVM.ConstInt(LLVM.Int32Type(), (ulong) i, false), LLVM.Int64Type(), "cast") }, "heap_ref_" + i));
			}*/
			
			return new StructHandle(builder, value);
		}

		public StructBuilder(LLVMModuleRef module, string name) {
			Module = module;

			Name = name;
		}
	}
}