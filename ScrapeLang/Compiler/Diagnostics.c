#include <stdio.h>
#include "Diagnostics.h"

typedef enum 
{
    Error = 0,
    Warning = 1,
    Message = 2,
} Severity;

typedef enum
{
    _Invalid_Type = 'Invalid Type!',
    _Null_Reference = 'Null Reference!',
    _Invalid_Argument = 'Invalid Argument!',
} Errors;