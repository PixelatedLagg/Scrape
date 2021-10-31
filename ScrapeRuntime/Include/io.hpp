#include <iostream>

namespace Standard {
    class Console {
        public:

        static void WriteLine(std::string stringtext);
        static void WriteLine(int inttext);
        static void Write(std::string stringtext);
        static void Write(int inttext);
        static std::string ReadLine();
    };
    class Convert {
        public:

        static std::string ToString(int value);
        static std::string ToString(float value);
        static std::string ToString(double value);
        static std::string ToString(char value);
        static std::string ToString(bool value);
    };
    class Random {
        public:

        static int Int(int min, int max);
    };
}