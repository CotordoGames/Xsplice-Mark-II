#include "objects.h"

obj loadedObjects[MAX_OBJECTS];
int ObjectCount = 0;

void spawnObject(int x, int y, obj object){
    if(ObjectCount >= MAX_OBJECTS) return;
    object.position = (Vector2){ x, y };
    loadedObjects[ObjectCount++] = object;
}

void deleteObject(int index){
    if (index < 0 || index >= ObjectCount) return;
    loadedObjects[index] = loadedObjects[--ObjectCount];
}

void DrawObjects(){
    for(int i = 0; i < ObjectCount; i++){
        loadedObjects[i].position.x += loadedObjects[i].velocity.x;
        loadedObjects[i].position.y += loadedObjects[i].velocity.y;
        DrawTexture(loadedObjects[i].texture, loadedObjects[i].position.x, loadedObjects[i].position.y, WHITE);
    }
}