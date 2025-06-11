PROJECTS := 1_echo
BIN_DIR := bin
MAELSTROM_URL := https://github.com/jepsen-io/maelstrom/releases/download/v0.2.3/maelstrom.tar.bz2

all: $(PROJECTS) maelstrom

maelstrom:
	mkdir -p maelstrom
	curl -sSL "$(MAELSTROM_URL)" | tar -xj --strip-components=1 -C maelstrom

$(BIN_DIR):
	mkdir -p $(BIN_DIR)

$(PROJECTS): | $(BIN_DIR)
	echo Building $@...
	cd $@ && go build -o ../$(BIN_DIR)/ .

test-echo: 1_echo maelstrom
	./maelstrom/maelstrom test -w echo --bin ./bin/echo --node-count 1 --time-limit 10

clean:
	rm -rf $(BIN_DIR) maelstrom

.PHONY: all clean $(PROJECTS)
