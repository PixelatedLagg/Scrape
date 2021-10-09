#ifndef Scrape_Diagnostics_h
#define Scrape_Diagnostics_h

#include <stdio.h>
#include <string.h>

typedef enum 
{
    Error = 0,
    Warning = 1,
    Message = 2,
} Severity;

typedef enum
{
    _Invalid_Type = 1,
    _Null_Reference = 2,
    _Invalid_Argument = 3,
} Errors;

#endif