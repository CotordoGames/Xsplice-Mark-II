#include "maps.h"

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