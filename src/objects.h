#ifndef OBJECTS_H_
#define OBJECTS_H_
#include <raylib.h>

#define MAX_OBJECTS 256

typedef struct{
    Texture2D texture;
    Vector2 position;
    Vector2 velocity;
} obj;

extern obj loadedObjects[MAX_OBJECTS];
extern int ObjectCount;

void spawnObject(int x, int y, obj object);
void deleteObject(int index);

void DrawObjects();

#endif