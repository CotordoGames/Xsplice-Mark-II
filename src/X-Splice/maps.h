#ifndef MAPS_H_
#define MAPS_H_
#include <stdio.h>
#include <stdlib.h>
#include <stdint.h>

typedef struct{
    uint8_t tile;
    uint8_t coll;
    uint8_t bright;
} map_tile;

typedef struct{
    char name[16];
    char author[16];
    uint16_t width;
    uint16_t height;
    uint8_t tileMap;
    uint8_t backGround;
    uint8_t backMusic;
    uint8_t objCount;
    map_tile *tileData;
} map;

#endif //MAPS_H_