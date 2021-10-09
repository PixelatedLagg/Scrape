#include <stdio.h>
#include <stdlib.h>

#include "Diagnostics.h"
#include "Objects.h"

void* Allocate_Object(Object *obj)
{
    switch ((*obj)._type)
    {
        case 'str':
            //store string
            break;

        case 'int': 
            //store integer
            break;

        default: 
            //error
            Log("Invalid Variable Type!", *obj);
            break;
    }
}