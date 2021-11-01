#include <string>

namespace Standard
{
    class Random 
    {
        public:
        static int Int(int min, int max);
        static std::string String(int stringType, int chars);
    };
}