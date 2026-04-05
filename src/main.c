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
#include <raymath.h>
#include "objects.h"

RenderTexture2D rendertex;

Texture2D player;

obj playerobj;

obj npc;

obj* player_inst;

obj* npc_inst;

int direction;

int speed = 2;

void Update(){
    DrawObjects();
    player_inst->velocity.x = Lerp(player_inst->velocity.x, (IsKeyDown(KEY_RIGHT) - IsKeyDown(KEY_LEFT)) * speed, 0.075f);
    npc_inst->velocity.x = Lerp(npc_inst->velocity.x, -(IsKeyDown(KEY_RIGHT) - IsKeyDown(KEY_LEFT)) * speed, 0.075f);
}

int main(){
    puts("hello, world!");

    // raylib init
    InitWindow(0, 0, "XSPLICE");
    SetTargetFPS(60);
    ToggleFullscreen();

    int WIDTH = GetMonitorWidth(0);
    int HEIGHT = GetMonitorHeight(0);

    int dstH = HEIGHT;
    int dstW = HEIGHT * 4 / 3;
    int offsetX = (WIDTH - dstW) / 2;

    // initialize the render texture to be 240p
    rendertex = LoadRenderTexture(320, 240);

    player = LoadTexture("assets/sprites/player.png");

    playerobj = (obj){ player, (Vector2){ 0, 0 }, (Vector2){ 0, 0 }, (Vector2){ 16, 16 }, true };
    npc = (obj){ player, (Vector2){ 0, 0 }, (Vector2){ 0, 0 }, (Vector2){ 16, 16 }, true };

    player_inst = spawnObject(0, 0, playerobj);
    npc_inst = spawnObject(298, 120, npc);

    // game loop
    while(!WindowShouldClose()){

        BeginTextureMode(rendertex);
            ClearBackground(ColorFromHSV(216, 1, 0.25f));
            Update();
        EndTextureMode();

        // draw the render texture with the game on it
        BeginDrawing();
            ClearBackground(BLACK);
            DrawTexturePro(
                rendertex.texture,
                (Rectangle){ 0, 0, 320, -240 },
                (Rectangle){ offsetX, 0, dstW, dstH },
                (Vector2){ 0, 0 },
                0.0f,
                WHITE
            );
        EndDrawing();
    }

    UnloadRenderTexture(rendertex);

    return 0;
}