#include <stdio.h>
#include "Objects.h"

void StoreVar(char Type, char Value)
{
    switch (Type)
    {
        case 'str':
            //store string
            break;

        case 'int': 
        {
            //store integer
            break;
        }
        default: 
        {
            //error
            printf("%s is not a valid type!", Type); //print error 1
            break;
        }
    }
}