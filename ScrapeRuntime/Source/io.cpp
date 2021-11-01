#include <io.hpp>
#include <fstream>
#include <iostream>

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
//File Methods
std::ofstream file;
void Standard::Console::OpenFile(std::string filename)
{
    file.open(filename);
}
void Standard::Console::CloseFile()
{
    file.close();
}
void Standard::Console::WriteToFile(std::string text)
{
    if (!file.is_open())
    {
        return;
        //user did not open file before attempting to write to it
        //will throw error later
    }
    file << text;
}
void Standard::Console::WriteLineToFile(std::string text)
{
    if (!file.is_open())
    {
        return;
        //user did not open file before attempting to write to it
        //will throw error later
    }
    file << text << std::endl;
}