#include <io.hpp>

namespace Standard {
	}

using namespace Standard;

namespace NAME {
	class Test {
		public:

		static int Fact(int i) {
			if (i == 1) {
				return 1;
			}

			return i * Fact(i - 1);
		}

	};

}

using namespace NAME;

int main() {
	Console::WriteLine("Enter something!");
	Console::WriteLine(Console::ReadLine());
}

