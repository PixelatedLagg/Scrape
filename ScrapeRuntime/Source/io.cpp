#include <io.hpp>

void Standard::Console::WriteLine(std::string stringtext)
{
    std::cout << stringtext << std::endl;
}
void WriteLine(int inttext)
{
    std::cout << std::to_string(inttext) << std::endl;
}
void Standard::Console::Write(std::string stringtext)
{
    std::cout << stringtext;
}
void Standard::Console::Write(int inttext)
{
    std::cout << std::to_string(inttext);
}
std::string Standard::Console::ReadLine()
{
    std::string output;
    std::cin >> output;
    return output;
}