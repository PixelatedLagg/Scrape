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
std::string Standard::Convert::ToString(int value)
{
    return std::to_string(value);
}
std::string Standard::Convert::ToString(float value)
{
    return std::to_string(value);
}
std::string Standard::Convert::ToString(double value)
{
    return std::to_string(value);
}
std::string Standard::Convert::ToString(char value)
{
    return std::to_string(value);
}
std::string Standard::Convert::ToString(bool value)
{
    return std::to_string(value);
}
int Standard::Random::Int(int min, int max)
{
    return (rand() % max) + min;
}