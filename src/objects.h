#ifndef OBJECTS_H_
#define OBJECTS_H_
#include <raylib.h>
#include <stdint.h>

#define MAX_OBJECTS 256

typedef struct{
    Texture2D texture;
    Vector2 position;
    Vector2 velocity;
    Vector2 size;
    uint32_t flags;
} obj;

extern obj loadedObjects[MAX_OBJECTS];
extern int ObjectCount;

obj* spawnObject(int x, int y, obj object);
void deleteObject(int index);

void DrawObjects();

#endif