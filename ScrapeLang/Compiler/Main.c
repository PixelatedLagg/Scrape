/*
Change Log:
started store var to store a var of a give type (if you want to change the way we store vars please do)
alot of errors to do with multilin char which is expected but i didnt have time to fix so please fix that
also github repo is private for now until we have a decent product ready
*/

#include <stdio.h>
#define FILE_EXTENSION .srp

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
            // todo later
            break;

        case 'int': 
        {
            break;
        }
        /* Would throw a error for to many chars in const ammount
        Potential fix would be to do a else if tree thing
        case 'float': 
        {
            break;
        }

        case 'double': 
        {
            break;
        }
        */

        default: {
            printf("error invalid type %s given", Type);
            break;
        }
    }
}