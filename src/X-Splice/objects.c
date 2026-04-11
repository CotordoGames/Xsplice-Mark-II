#include "objects.h"
#include <stdio.h>
#include <raylib.h>

obj loadedObjects[MAX_OBJECTS];
int ObjectCount = 0;
bool debug = false;


obj* spawnObject(int x, int y, obj object){
    if(ObjectCount >= MAX_OBJECTS) return NULL;
    object.position = (Vector2){ x, y };
    loadedObjects[ObjectCount] = object;
    return &loadedObjects[ObjectCount++];
}

void deleteObject(int index){
    if (index < 0 || index >= ObjectCount) return;
    loadedObjects[index] = loadedObjects[--ObjectCount];
}

void DrawObjects(){

    if(IsKeyPressed(KEY_F1)){
        debug = !debug;
    }

    for(int i = 0; i < ObjectCount; i++){
        bool BlockedX = false;
        bool BlockedY = false;
        // check for and update flags / flag based conditions
        if(loadedObjects[i].flags & X_GRAVITY){
            loadedObjects[i].velocity.y += 0.25f;
            if(loadedObjects[i].velocity.y > 8){
                loadedObjects[i].velocity.y = 8;
            }
        }

        if(loadedObjects[i].flags & X_SOLID){ // && loadedObjects[i].velocity.x != 0 && loadedObjects[i].velocity.y != 0
            for(int x = 0; x < ObjectCount; x++){
                if(loadedObjects[x].flags & X_SOLID){

                    if(i == x) continue;

                    float nextX = loadedObjects[i].position.x + loadedObjects[i].velocity.x;
                    float nextY = loadedObjects[i].position.y + loadedObjects[i].velocity.y;

                    Rectangle FutureX = { // define a rect that is one generation ahead of what the X should be to check if it will be touching something next frame
                        nextX + loadedObjects[i].colliderOffset.x,
                        loadedObjects[i].position.y + loadedObjects[i].colliderOffset.y,
                        loadedObjects[i].size.x,
                        loadedObjects[i].size.y
                    };

                    Rectangle FutureY ={ // same as  ^ for Y
                        loadedObjects[i].position.x + loadedObjects[i].colliderOffset.x,
                        nextY + loadedObjects[i].colliderOffset.y,
                        loadedObjects[i].size.x,
                        loadedObjects[i].size.y
                    };

                    Rectangle HitboxB = { // define the hitbox of the collider using premade variables
                        loadedObjects[x].position.x + loadedObjects[x].colliderOffset.x,
                        loadedObjects[x].position.y + loadedObjects[x].colliderOffset.y,
                        loadedObjects[x].size.x,
                        loadedObjects[x].size.y
                    };

                    //actually check the collision; this is raylibs default functions
                    if(CheckCollisionRecs(FutureX, HitboxB)) BlockedX = true;
                    if(CheckCollisionRecs(FutureY, HitboxB)) BlockedY = true;

                }
            }
        }

        if(!BlockedX){
            loadedObjects[i].position.x += loadedObjects[i].velocity.x;
        } else {
            loadedObjects[i].velocity.x = 0;
        }
        if(!BlockedY){
            loadedObjects[i].position.y += loadedObjects[i].velocity.y;
        } else {
            loadedObjects[i].velocity.y = 0;
        }

        if(loadedObjects[i].flags & X_VISIBLE){
            DrawTexture(loadedObjects[i].texture, loadedObjects[i].position.x, loadedObjects[i].position.y, WHITE);
        }
        if(debug){
            DrawRectangleLinesEx((Rectangle){loadedObjects[i].position.x + loadedObjects[i].colliderOffset.x, loadedObjects[i].position.y + loadedObjects[i].colliderOffset.y, loadedObjects[i].size.x, loadedObjects[i].size.y}, 1, GREEN);
            DrawRectangle(loadedObjects[i].position.x + loadedObjects[i].colliderOffset.x, loadedObjects[i].position.y + loadedObjects[i].colliderOffset.y, loadedObjects[i].size.x, loadedObjects[i].size.y, (Color){0, 255, 0, 64});
        }
    }
}