#include <io.hpp>

void Standard::Console::WriteLine(std::string text)
{
    std::cout << text << std::endl;
}
void Standard::Console::Write(std::string text)
{
    std::cout << text;
}
std::string Standard::Console::ReadLine()
{
    std::string output;
    std::cin >> output;
    return output;
}