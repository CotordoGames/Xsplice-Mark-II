#include "maps.h"
#include "objects.h"
#include "camera.h"

map *currentMap = NULL;

map *readMap(char *path){
    FILE *fptr;

    fptr = fopen(path, "rb");

    if(!fptr) return NULL;

    map *m = malloc(sizeof(map));

    uint8_t magic[4];
    fread(magic, 1, 4, fptr);  // skip XSM1

    fread(m->name, 1, 16, fptr);
    fread(m->author, 1, 16, fptr);
    fread(&m->width, 2, 1, fptr);
    fread(&m->height, 2, 1, fptr);
    fread(&m->tileMap, 1, 1, fptr);
    fread(&m->backGround, 1, 1, fptr);
    fread(&m->backMusic, 1, 1, fptr);
    fread(&m->objCount, 1, 1, fptr);

    int tileCount = m->width * m->height;
    m->tileData = malloc(sizeof(map_tile) * tileCount);
    fread(m->tileData, sizeof(map_tile), tileCount, fptr);

    char tpath[32];
    sprintf(tpath, "assets/sprites/tm%d.png", m->tileMap);
    m->tileSet = LoadTexture(tpath);
    char bpath[32];
    sprintf(bpath, "assets/sprites/bg%d.png", m->backGround);
    m->bg_tex = LoadTexture(bpath);

    for(int i = 0; i < m->objCount; i++){
        uint16_t x, y, w, h;
        fread(&x, 2, 1, fptr);
        fread(&y, 2, 1, fptr);
        fread(&w, 2, 1, fptr);
        fread(&h, 2, 1, fptr);

        obj collisionObject = {0};
        collisionObject.size = (Vector2){w * 16, h * 16};
        collisionObject.flags = X_SOLID;

        spawnObject(x * 16, y * 16, collisionObject);

    }

    return m;
}

void setMap(map *m){
    if(currentMap) freeMap(currentMap);
    currentMap = m;
}

void freeMap(map *map){
    if(!map) return;
    UnloadTexture(map->tileSet);
    free(map->tileData);
    free(map);
}

void drawMap(){
    if(!currentMap) return;

    //reason mods are here is to check if the screen's position is a multiple of 32 so it can reset the position to avoid badk shit fro happening
    int offX = (int)(cam.target.x * 0.5f) % currentMap->bg_tex.width;
    int offY = (int)(cam.target.y * 0.5f) % currentMap->bg_tex.height;

    float startX = cam.target.x - 160 - offX;
    float startY = cam.target.y - 120 - offY;

    for(int i = -1; i <= 240/currentMap->bg_tex.height + 1; i++){
        for(int x = -1; x <= 320/currentMap->bg_tex.width + 1; x++){
            DrawTexture(currentMap->bg_tex,
            startX + x * currentMap->bg_tex.width,
            startY + i * currentMap->bg_tex.height,
            WHITE);
        }
    }
    for(int y = 0; y < currentMap->height; y++){
        for(int x = 0; x < currentMap->width; x++){
            map_tile t = currentMap->tileData[y * currentMap->width + x];
            if(t.tile == 0) continue;

            int idx = t.tile;
            Rectangle src = {
                (idx % 16) * 16,
                (idx / 16) * 16,
                16, 16
            };
            Rectangle dst = { x * 16, y * 16, 16, 16 };

            DrawTexturePro(currentMap->tileSet, src, dst, (Vector2){ 0, 0 }, 0, WHITE);
        }
    }
}