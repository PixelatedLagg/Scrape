#include <stdio.h>
#include "Diagnostics.h"

void Log(char _error[], Object _object)
{
    printf("%s \n Object: name \"%s\" type \"%s\"", _error, _object._name, _object._type);
}

typedef enum
{
    _Invalid_Type = 'Invalid Type!',
    _Null_Reference = 'Null Reference!',
    _Invalid_Argument = 'Invalid Argument!',
} Errors;