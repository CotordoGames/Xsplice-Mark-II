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
#include "X-Splice/maps.h"
#include "X-Splice/camera.h"

RenderTexture2D rendertex;

Texture2D player;
Texture2D bgi;
Texture2D ground;

// objects
obj playerobj;
obj npc;
obj bg;
obj groundobj;

// object instances
obj* bg_inst;
obj* player_inst;
obj* npc_inst;
obj* ground_inst;

int direction;

int speed = 2;

void start(){
    initCamera(0, 0);
    map *m = readMap("assets/maps/map01.xsm");
    if(!m) puts("map failed to load!");
    else printf("map: %dx%d tilemap:%d\n", m->width, m->height, m->tileMap);
    setMap(m);

    player = LoadTexture("assets/sprites/player.png");

    bgi = LoadTexture("assets/sprites/bg.png");
    ground = LoadTexture("assets/sprites/ground.png");

    playerobj = (obj){ player, (Vector2){ 0, 0 }, (Vector2){ 0, 0 }, (Vector2){ 8, 7 }, (Vector2){ 8, 16 }, X_VISIBLE | X_SOLID | X_GRAVITY };
    npc = (obj){ player, (Vector2){ 0, 0 }, (Vector2){ 0, 0 }, (Vector2){ 8, 8 }, (Vector2){ 8, 16 }, X_VISIBLE | X_SOLID };
    groundobj = (obj){ ground, (Vector2){ 0, 192 }, (Vector2){ 0, 0 }, (Vector2){ 320, 48 }, (Vector2){ 0, 0 }, X_SOLID};


    player_inst = spawnObject(0, 0, playerobj);
    npc_inst = spawnObject(298, 120, npc);
    ground_inst = spawnObject(0, 192, groundobj);
}

void Update(){
    // draw current map

    if(player_inst->velocity.y == 0){
        player_inst->velocity.x = Lerp(player_inst->velocity.x, (IsKeyDown(KEY_RIGHT) - IsKeyDown(KEY_LEFT)) * speed, 0.085f);
        if(IsKeyPressed(KEY_Z)){
            player_inst-> velocity.y = -6;
        }
    }
    else{
        player_inst->velocity.x = Lerp(player_inst->velocity.x, (IsKeyDown(KEY_RIGHT) - IsKeyDown(KEY_LEFT)) * speed * 1.25, 0.04f);
    }
    if(player_inst->velocity.y < -0.1f && IsKeyUp(KEY_Z)){
        player_inst->velocity.y /= 1.25;
    }

    npc_inst->velocity.x = Lerp(npc_inst->velocity.x, (IsKeyDown(KEY_D) - IsKeyDown(KEY_A)) * speed, 0.075f);
    npc_inst->velocity.y = Lerp(npc_inst->velocity.y, -(IsKeyDown(KEY_W) - IsKeyDown(KEY_S)) * speed, 0.075f);
    Vector2 CamFollow = {
        player_inst->position.x + (player_inst->size.x / 2),
        player_inst->position.y + (player_inst->size.y / 2)
    };
    updateCamera(CamFollow);
    beginCameraDraw();
    drawMap();
    DrawObjects();
    endCameraDraw();
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