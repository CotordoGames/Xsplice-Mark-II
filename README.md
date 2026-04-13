## X-Splice Mark II game engine

X-Splice is a game engine that focus on simplicity and access to objects. heres what makes it tick:

## How Objects Work

There is any array of "loaded objects", where an "object" is made up of an X, Y, Collider Width, Collider Height, Collider Offset X, Collider Offset Y, and a texture. basically, you can use "load object" to... well... load an object. after it adds the values you specify into the loaded objects array, it returns a pointer to the loaded object so you can access it easily by setting an obj* to `LoadObject(x, y, obj)`. and object are obviosly just the "presets", collections of data that make up an object that can be spawned

## collision

 you dont really need to know anything other than the fact that you need the `X_SOLID` flag on an object for it to be... well... solid!

 ## flags

 in the object struct, there is a `uint32_t flags`. in objects.h, flags such as `X_SOLID`, `X_VISIBLE`, ect. are defined as bit shifts, and then how the oject gets updated changes depending on the flags; this is what makes this engine the most modular.

 ## maps

 renders the tiles with a special lil for loop and spawns objects for collision(you manually define collision rects in editor)
