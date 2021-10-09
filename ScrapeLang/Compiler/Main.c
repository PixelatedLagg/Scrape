#include <stdio.h>
#define FILE_EXTENSION .srp
#define FILE_EXTENSION .scrape

void storeVar(char, char);

int Main()
{
    storeVar('st', 'test');
    return 0;
}

void storeVar(char Type, char Value)
{
    switch (Type)
    {
        case 'str':
            //add string
            break;

        case 'int': 
        {
            //add integer
            break;
        }
        default: 
        {
            //error
            printf("%s is not a valid type!", Type);
            break;
        }
    }
}