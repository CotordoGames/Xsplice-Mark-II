#ifndef OBJECTS_H_
#define OBJECTS_H_
#include <raylib.h>
#include <stdint.h>

#define MAX_OBJECTS 256
#define X_SOLID (1 << 0)
#define X_GRAVITY (1 << 1)
#define X_VISIBLE (1 << 2)
#define X_TRIGGER (1 << 3)

typedef struct{
    Texture2D texture;
    Vector2 position;
    Vector2 velocity;
    Vector2 size;
    Vector2 colliderOffset;
    uint32_t flags;
} obj;

extern bool debug;

extern obj loadedObjects[MAX_OBJECTS];
extern int ObjectCount;

obj* spawnObject(int x, int y, obj object);
void deleteObject(int index);

void DrawObjects();

#endif