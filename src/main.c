// X-SPLICE-ENGINE MARK II
//
// BY DEAN "DJ" BUDDEN
//
// FOR USAGE IN CUBON MINI
//
// THIS CODE IS MINE BITCH
//
// HANDS OFF
//
#include <stdio.h>
#include <raylib.h>

RenderTexture2D rendertex;

int main(){
    puts("hello, world!");

    // raylib init
    InitWindow(640, 480, "XSPLICE");
    SetTargetFPS(60);
    ToggleFullscreen();

    rendertex = LoadRenderTexture(320, 240);

    // game loop
    while(!WindowShouldClose()){

        BeginTextureMode(rendertex);
            ClearBackground(BLACK);
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