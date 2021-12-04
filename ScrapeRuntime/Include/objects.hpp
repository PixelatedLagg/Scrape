#include <map>

using std::map;

class S_Object;

typedef struct S_GCHandle {
    S_Object* Object;

    int RefCount;

    S_GCHandle* Prev;

    S_GCHandle* Next;
} S_GCHandle;

class S_Object {
    public:

    S_GCHandle* S_Handle;

    map<string, S_Object*> S_Properties;

    S_Object* operator[](string key);

    void S_Object::Ref();

    void S_Object::Unref();
};

class S_GC {
    public:

    S_GCHandle* First = nullptr;

    S_GCHandle* Last = nullptr;

    template <class T>
    T* Alloc();

    void Sweep();
}