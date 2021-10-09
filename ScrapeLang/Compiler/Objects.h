#ifndef Scrape_Objects_h
#define Scrape_Objects_h

#include <stdio.h>

typedef enum
{
    _int = 0,
    _string = 1,
} Types;

typedef struct
{
    Types _type;
    char _name;
} Object;

void StoreVar(char Type, char Value);

#endif