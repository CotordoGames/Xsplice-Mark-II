#include "camera.h"
#include "maps.h"
#include <raymath.h>

Camera2D cam = {0};

void initCamera(float x, float y){
    cam.target = (Vector2){x, y};
    cam.offset = (Vector2){ 160, 120 };
    cam.rotation = 0;
    cam.zoom = 1;
}

void updateCamera(Vector2 targetPosition){
    float dt = GetFrameTime();

    float tx = 1.0f - powf(1.0f - 0.25f, dt * 60.0f);
    cam.target.x = Clamp(Lerp(cam.target.x, (int)targetPosition.x, tx), 160,  currentMap->width * 16 - 160);
    //cam.target.x = Clamp((int)targetPosition.x, 160,  currentMap->width * 16 - 160);


    float ty = 1.0f - powf(1.0f - 0.05f, dt * 60.0f);
    cam.target.y = Clamp(Lerp(cam.target.y, (int)targetPosition.y, ty), 120, currentMap->height * 16 - 120);
    //cam.target.y = Clamp((int)targetPosition.y, 120, currentMap->height * 16 - 120);
}

void beginCameraDraw(){
    BeginMode2D(cam);
}

void endCameraDraw(){
    EndMode2D();
}