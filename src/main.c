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

// object instances
obj* bg_inst;
obj* player_inst;
obj* npc_inst;

int direction;

int speed = 3;

void start(){
    initCamera(0, 0);
    map *m = readMap("assets/maps/level_01.xsm");
    if(!m) puts("map failed to load!");
    else printf("map: %dx%d tilemap:%d\n", m->width, m->height, m->tileMap);
    setMap(m);

    player = LoadTexture("assets/sprites/player.png");

    bgi = LoadTexture("assets/sprites/bg.png");
    ground = LoadTexture("assets/sprites/ground.png");

    playerobj = (obj){ player, (Vector2){ 0, 0 }, (Vector2){ 0, 0 }, (Vector2){ 8, 7 }, (Vector2){ 8, 16 }, X_VISIBLE | X_SOLID | X_GRAVITY };
    npc = (obj){ player, (Vector2){ 0, 0 }, (Vector2){ 0, 0 }, (Vector2){ 8, 8 }, (Vector2){ 8, 16 }, X_VISIBLE | X_SOLID };


    player_inst = spawnObject(160 - 12, 120, playerobj);
    npc_inst = spawnObject(298, 120, npc);
}

void Update(){
    // draw current map
    float dt = GetFrameTime();

    if(player_inst->velocity.y == 0){
        if((IsKeyDown(KEY_RIGHT) - IsKeyDown(KEY_LEFT)) != 0){
            float t = 1.0f - powf(1.0f - 0.085f, dt * 60.0f);
            player_inst->velocity.x = Lerp(player_inst->velocity.x, (IsKeyDown(KEY_RIGHT) - IsKeyDown(KEY_LEFT)) * speed, t);
        } else{
            float t = 1.0f - powf(1.0f - 0.125f, dt * 60.0f);
            player_inst->velocity.x = Lerp(player_inst->velocity.x, (IsKeyDown(KEY_RIGHT) - IsKeyDown(KEY_LEFT)) * speed, t);
        }
        if(IsKeyPressed(KEY_Z)){
            player_inst-> velocity.y = -7;
        }
    }
    else{
        float t = 1.0f - powf(1.0f - 0.04f, dt * 60.0f);
        player_inst->velocity.x = Lerp(player_inst->velocity.x, (IsKeyDown(KEY_RIGHT) - IsKeyDown(KEY_LEFT)) * speed * 1.25, t);
    }
    if(player_inst->velocity.y < -0.1f && IsKeyUp(KEY_Z)){
        player_inst->velocity.y /= 1.25;
    }

    npc_inst->velocity.x = Lerp(npc_inst->velocity.x, (IsKeyDown(KEY_D) - IsKeyDown(KEY_A)) * speed, 0.075f * dt * 60);
    npc_inst->velocity.y = Lerp(npc_inst->velocity.y, -(IsKeyDown(KEY_W) - IsKeyDown(KEY_S)) * speed, 0.075f * dt * 60);
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
    
    SetConfigFlags(FLAG_WINDOW_RESIZABLE | FLAG_VSYNC_HINT);
    

    // raylib init
    InitWindow(1280, 720, "XSPLICE");
    SetTargetFPS(0);

    // initialize the render texture to be 240p
    rendertex = LoadRenderTexture(320, 240);

    start();

    SetTextureFilter(rendertex.texture, TEXTURE_FILTER_POINT);

    // game loop
    while(!WindowShouldClose()){
        int WIDTH = GetScreenWidth();
        int HEIGHT = GetScreenHeight();

        int dstH = HEIGHT;
        int dstW = HEIGHT * 4 / 3;
        int offsetX = (WIDTH - dstW) / 2;


        if(IsKeyPressed(KEY_F11)){
            ToggleFullscreen();
        }

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