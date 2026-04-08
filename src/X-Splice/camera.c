#include "camera.h"

static Camera2D cam = {0};

void initCamera(float x, float y){
    cam.target = (Vector2){x, y};
    cam.offset = (Vector2){ 160, 120 };
    cam.rotation = 0;
    cam.zoom = 1;
}

void updateCamera(Vector2 targetPosition){
    cam.target.x = (int)targetPosition.x;
    cam.target.y = (int)targetPosition.y;
}

void beginCameraDraw(){
    BeginMode2D(cam);
}

void endCameraDraw(){
    EndMode2D();
}