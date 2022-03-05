#include <stdio.h>

// LLVM shadow stack

typedef struct {
	int NumRoots;
	int NumMeta;

	const void** Meta;
} FrameMap;

typedef struct StackEntry {
	struct StackEntry* Next;
	
	FrameMap* Map;

	void** Roots;
} StackEntry;

StackEntry* llvm_gc_root_chain;

typedef struct {
	int i;
	int w;
} Test;

void AddrTest(Test* test) {
	printf("Struct address test: %p\n", (*test));
}

void IncRef(Test* test) {
	printf("Struct ref++ address: %p\n", test);

	// printf("Struct i member: %i\n", (*test).i);
}

void DecRef(Test* test) {
	printf("Struct ref-- address: %p\n", test);

	// printf("Struct i member: %i\n", (*test).i);
}

void Visitor(void** root, const void* meta) {
	printf("Visiting %p\n", root);
}

void VisitGCRoots(/*void (*VisitorDelegate)(void** root, const void* meta)*/) {
	for (StackEntry* entry = llvm_gc_root_chain; entry; entry = entry->Next) {
		for (int i = 0; i < entry->Map->NumRoots; i++) {
			Visitor(entry->Roots[i], 0);
		}
	}
}