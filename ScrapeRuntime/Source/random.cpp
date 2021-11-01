#include <random.hpp>
#include <algorithm>

int Standard::Random::Int(int min, int max)
{
    return (rand() % max) + min;
}
std::string Standard::Random::String(int stringType, int chars)
{
    switch (stringType)
    {
        case 0:
        {
            auto randchar = []() -> char
            {
                const char charset[] =
                "ABCDEFGHIJKLMNOPQRSTUVWXYZ"
                "abcdefghijklmnopqrstuvwxyz";
                const size_t max_index = (sizeof(charset) - 1);
                return charset[rand() % max_index];
            };
            std::string str(chars, 0);
            std::generate_n( str.begin(), chars, randchar);
            return str;
        }
        case 1:
        {
            auto randchar = []() -> char
            {
                const char charset[] = "abcdefghijklmnopqrstuvwxyz";
                const size_t max_index = (sizeof(charset) - 1);
                return charset[rand() % max_index];
            };
            std::string str(chars, 0);
            std::generate_n( str.begin(), chars, randchar);
            return str;
        }
        case 2:
        {
            auto randchar = []() -> char
            {
                const char charset[] = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
                const size_t max_index = (sizeof(charset) - 1);
                return charset[rand() % max_index];
            };
            std::string str(chars, 0);
            std::generate_n( str.begin(), chars, randchar);
            return str;
        }
    }
}