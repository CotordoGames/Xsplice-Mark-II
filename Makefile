# XSPLICE ENGINE MARK II
# Builds all .c files in src/ — headers discovered automatically

CC      = gcc
TARGET  = XSPLICE
SRCDIR  = src
OBJDIR  = obj

SRCS    = $(wildcard $(SRCDIR)/*.c)
OBJS    = $(patsubst $(SRCDIR)/%.c, $(OBJDIR)/%.o, $(SRCS))

CFLAGS  = -Wall -Wextra -I$(SRCDIR) -O2
LDFLAGS = -lraylib -lm -ldl -lpthread -lX11

# ── platform-specific ──────────────────────────────────────────
UNAME := $(shell uname -s)
ifeq ($(UNAME), Linux)
    LDFLAGS += -lGL
endif
ifeq ($(UNAME), Darwin)
    LDFLAGS += -framework OpenGL -framework Cocoa -framework IOKit
endif
# MinGW / Windows
ifneq (,$(findstring MINGW,$(UNAME)))
    LDFLAGS += -lopengl32 -lgdi32 -lwinmm
    TARGET  := $(TARGET).exe
endif

# ── rules ──────────────────────────────────────────────────────
.PHONY: all clean run

all: $(OBJDIR) $(TARGET)

$(OBJDIR):
	mkdir -p $(OBJDIR)

$(TARGET): $(OBJS)
	$(CC) $(OBJS) -o $@ $(LDFLAGS)
	@echo "linked → $(TARGET)"

$(OBJDIR)/%.o: $(SRCDIR)/%.c
	$(CC) $(CFLAGS) -c $< -o $@

clean:
	rm -rf $(OBJDIR) $(TARGET)

run: all
	./$(TARGET)