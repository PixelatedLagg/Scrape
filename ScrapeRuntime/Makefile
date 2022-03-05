CXX = gcc

INCLUDE = ./Include
SRC = ./Source

OBJS = $(patsubst $(SRC)/%.c,%.o,$(wildcard $(SRC)/*.c))

%.o:
	$(CXX) $(SRC)/$(patsubst %.o,%,$@).c -c -I$(INCLUDE) -o obj/$(patsubst %.o,%,$@).o -fPIC

$(SRC)/%.cpp: $(patsubst %.c,%,$@).o

all: $(OBJS)
	

clean:
	rm -rf obj	