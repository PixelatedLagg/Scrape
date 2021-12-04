#include <scrape.hpp>

namespace Standard {
			}

using namespace Standard;

namespace NAME {
	class Test : S_Object {
		public:

		void Empty() {
		}

	};

}

using namespace NAME;

int main() {
	Test* t = S_GC::Alloc<Test>());
	if ((*t)["Prop"] != nullptr) (*t)["Prop"]->S_Handle->Unref();
(*t)["Prop"] = 15;
	if ((*t)["Prop"] != nullptr) (*t)["Prop"]->S_Handle->Unref();
(*t)["Prop"] = 20;
	Console::WriteLine((*t)["Wow"]);
	return 0;
}

