// X-SPLICE-ENGINE MARK II
//
// BY DEAN "DJ" BUDDEN
//
// FOR USAGE IN CUBON MINI
//
// THIS CODE IS MINE- NOT YOURS!
//
// HANDS OFF
//
#include <stdio.h>
#include <raylib.h>
#include "objects.h"

RenderTexture2D rendertex;

Texture2D player;

obj playerobj;

void Update(){
    DrawObjects();
    obj APO = loadedObjects[0];
    APO.position.x++;
    loadedObjects[0] = APO;
}

int main(){
    puts("hello, world!");

    // raylib init
    InitWindow(640, 480, "XSPLICE");
    SetTargetFPS(60);
    ToggleFullscreen();

    rendertex = LoadRenderTexture(320, 240);

    player = LoadTexture("assets/sprites/player.png");

    playerobj = (obj){ player, (Vector2){ 0, 0 }, (Vector2){ 0, 0 } };

    spawnObject(0, 0, playerobj);

    // game loop
    while(!WindowShouldClose()){

        BeginTextureMode(rendertex);
            ClearBackground(BLACK);
            Update();
        EndTextureMode();

        BeginDrawing();
            ClearBackground(BLACK);
            DrawTexturePro(
                rendertex.texture,
                (Rectangle){ 0, 0, 320, -240 },
                (Rectangle){ 0, 0, 640, 480 },
                (Vector2){ 0, 0 },
                0.0f,
                WHITE
            );
        EndDrawing();
    }

    UnloadRenderTexture(rendertex);

    return 0;
}