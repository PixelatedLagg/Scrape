#include <iostream>

namespace Standard {
    class Console {
        public:

        static void WriteLine(std::string text);
        static void Write(std::string text);
        static std::string ReadLine();
    };
}