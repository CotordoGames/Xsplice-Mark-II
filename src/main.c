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
#include "X-Splice/objects.h"

RenderTexture2D rendertex;

Texture2D player;

Texture2D bgi;

// objects
obj playerobj;
obj npc;
obj bg;

// object instances
obj* bg_inst;
obj* player_inst;
obj* npc_inst;

int direction;

int speed = 2;

void start(){
    player = LoadTexture("assets/sprites/player.png");

    bgi = LoadTexture("assets/sprites/bg.png");

    playerobj = (obj){ player, (Vector2){ 0, 0 }, (Vector2){ 0, 0 }, (Vector2){ 8, 8 }, (Vector2){ 8, 16 }, X_VISIBLE | X_SOLID | X_GRAVITY };
    npc = (obj){ player, (Vector2){ 0, 0 }, (Vector2){ 0, 0 }, (Vector2){ 8, 8 }, (Vector2){ 8, 16 }, X_VISIBLE | X_SOLID };
    bg = (obj){ bgi, (Vector2){ 0, 0 }, (Vector2){ 0, 0 }, (Vector2){ 320, 240 }, (Vector2){ 0, 0 }, X_VISIBLE};

    bg_inst = spawnObject(0, 0, bg);
    player_inst = spawnObject(0, 0, playerobj);
    npc_inst = spawnObject(298, 120, npc);
}

void Update(){
    player_inst->velocity.x = Lerp(player_inst->velocity.x, (IsKeyDown(KEY_RIGHT) - IsKeyDown(KEY_LEFT)) * speed, 0.075f);

    if(player_inst->velocity.y == 0 && IsKeyPressed(KEY_Z)){
        player_inst-> velocity.y = -4;
    }

    npc_inst->velocity.x = Lerp(npc_inst->velocity.x, (IsKeyDown(KEY_D) - IsKeyDown(KEY_A)) * speed, 0.075f);
    npc_inst->velocity.y = Lerp(npc_inst->velocity.y, -(IsKeyDown(KEY_W) - IsKeyDown(KEY_S)) * speed, 0.075f);
    DrawObjects();
    Color translucent = { 255, 255, 255, 128};
    DrawText("X-SPLICE MK II ENGINE -- V A0.1 BUILD\nNOT FOR PUBLIC REPRODUCTION", 0, 0, 8, translucent);
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

    start();

    // game loop
    while(!WindowShouldClose()){

        // draw the game at true resolution
        BeginTextureMode(rendertex);
            ClearBackground(ColorFromHSV(216, 1, 0.25f)); //placeholder
            Update();
        EndTextureMode();

        // draw the render texture with the game on it
        BeginDrawing();
            ClearBackground(BLACK);
            DrawTexturePro(
                rendertex.texture,
                (Rectangle){ 0, 0, 320, -240 },
                (Rectangle){ offsetX, 0, dstW, dstH }, // draws it at full screen resolution
                (Vector2){ 0, 0 },
                0.0f,
                WHITE
            );
        EndDrawing();
    }

    UnloadRenderTexture(rendertex);

    return 0;
}