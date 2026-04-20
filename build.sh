#!/bin/bash
mkdir -p bin/obj
for f in src/*.c src/X-Splice/*.c; do
    clang -Wall -Wextra -Isrc -Isrc/X-Splice -O2 -c "$f" -o "bin/obj/$(basename ${f%.c}).o"
done
clang bin/obj/*.o -o bin/XSPLICE -lraylib -lm -lpthread -lGL -lX11 -ldl -mwindows
./bin/XSPLICE