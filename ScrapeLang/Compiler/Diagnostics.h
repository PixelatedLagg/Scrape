#ifndef Scrape_Diagnostics_h
#define Scrape_Diagnostics_h

#include <stdio.h>
#include <string.h>

#include "Objects.h"

void Log(char _error[], Object _object);

typedef enum
{
    _Invalid_Type = 1,
    _Null_Reference = 2,
    _Invalid_Argument = 3,
} Errors;

#endif