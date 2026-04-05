# XSPLICE ENGINE MARK II
# Builds all .c files in src/ — headers discovered automatically

CC      = clang
TARGET  = XSPLICE
SRCDIR  = src
OBJDIR  = bin/obj
BINDIR  = bin

SRCS    = $(wildcard $(SRCDIR)/*.c)
OBJS    = $(patsubst $(SRCDIR)/%.c, $(OBJDIR)/%.o, $(SRCS))

CFLAGS  = -Wall -Wextra -I$(SRCDIR) -O2
LDFLAGS = -lraylib -lm -lpthread

# --- platform-specific ---
ifeq ($(OS), Windows_NT)
    LDFLAGS += -lopengl32 -lgdi32 -lwinmm
    TARGET  := $(BINDIR)/$(TARGET).exe
else
    UNAME := $(shell uname -s)
    ifeq ($(UNAME), Linux)
        LDFLAGS += -lGL -lX11 -ldl
    endif
    ifeq ($(UNAME), Darwin)
        LDFLAGS += -framework OpenGL -framework Cocoa -framework IOKit
    endif
    TARGET := $(BINDIR)/$(TARGET)
endif

# --- rules ---
.PHONY: all clean run

all: $(OBJDIR) $(TARGET)

$(OBJDIR):
ifeq ($(OS), Windows_NT)
	if not exist bin\obj mkdir bin\obj
else
	mkdir -p $(OBJDIR)
endif

$(TARGET): $(OBJS)
	$(CC) $(OBJS) -o $@ $(LDFLAGS)
	@echo "linked → $(TARGET)"

$(OBJDIR)/%.o: $(SRCDIR)/%.c
	$(CC) $(CFLAGS) -c $< -o $@

clean:
ifeq ($(OS), Windows_NT)
	if exist $(OBJDIR) rmdir /s /q $(OBJDIR)
	if exist $(TARGET) del /q $(TARGET)
else
	rm -rf $(OBJDIR) $(TARGET)
endif

run: all
	./$(TARGET)