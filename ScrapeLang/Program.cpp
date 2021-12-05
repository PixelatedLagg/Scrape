#include <scrape.hpp>

using namespace Standard;

namespace NAME {
	}

using namespace NAME;

int main() {
	(*Console)["OpenFile"]("test.txt");
	(*Console)["WriteToFile"]("Hello World!");
	(*Console)["CloseFile"]();
	return 0;
}

