#!/bin/bash
mkdir -p bin/obj
clang -Wall -Wextra -Isrc -Isrc/X-Splice -O2 -c src/main.c -o bin/obj/main.o
clang -Wall -Wextra -Isrc -Isrc/X-Splice -O2 -c src/X-Splice/objects.c -o bin/obj/objects.o
clang bin/obj/main.o bin/obj/objects.o -o bin/XSPLICE -lraylib -lm -lpthread -lGL -lX11 -ldl
./bin/XSPLICE