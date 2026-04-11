#ifndef CAMERA_H_
#define CAMERA_H_
#include <raylib.h>

extern Camera2D cam;
void initCamera(float x, float y);
void updateCamera(Vector2 targetPosition);
void beginCameraDraw();
void endCameraDraw();

#endif