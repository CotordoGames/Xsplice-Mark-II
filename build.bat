@echo off
if not exist bin\obj mkdir bin\obj
for %%f in (src\*.c src\X-Splice\*.c) do (
    clang -Wall -Wextra -Isrc -Isrc/X-Splice -O2 -c %%f -o bin/obj/%%~nf.o
)
clang bin/obj/*.o -o bin/XSPLICE.exe -lraylib -lm -lpthread -lopengl32 -lgdi32 -lwinmm -mwindows
bin\XSPLICE.exe