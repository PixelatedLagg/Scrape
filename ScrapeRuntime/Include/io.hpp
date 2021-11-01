#include <iostream>

namespace Standard 
{
    class Console 
    {
        public:
        static void WriteLine(std::string stringtext);
        static void WriteLine(int inttext);
        static void Write(std::string stringtext);
        static void Write(int inttext);
        static std::string ReadLine();
    };
}