@echo off
if not exist bin\obj mkdir bin\obj
clang -Wall -Wextra -Isrc -Isrc/X-Splice -O2 -c src/main.c -o bin/obj/main.o
clang -Wall -Wextra -Isrc -Isrc/X-Splice -O2 -c src/X-Splice/objects.c -o bin/obj/objects.o
clang bin/obj/main.o bin/obj/objects.o -o bin/XSPLICE.exe -lraylib -lm -lpthread -lopengl32 -lgdi32 -lwinmm
bin\XSPLICE.exe