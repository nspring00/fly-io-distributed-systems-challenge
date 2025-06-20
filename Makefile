PROJECTS := 1_echo 2_unique_ids 3_broadcast
BIN_DIR := bin
MAELSTROM_URL := https://github.com/jepsen-io/maelstrom/releases/download/v0.2.3/maelstrom.tar.bz2

all: $(PROJECTS) maelstrom

maelstrom:
	@echo Downloading Maelstrom...
	@mkdir -p maelstrom
	@curl -sSL "$(MAELSTROM_URL)" | tar -xj --strip-components=1 -C maelstrom

$(BIN_DIR):
	@mkdir -p $(BIN_DIR)

$(PROJECTS): | $(BIN_DIR)
	@echo Building $@...
	@cd $@ && go build -o ../$(BIN_DIR)/ .

test-1: 1_echo maelstrom
	./maelstrom/maelstrom test -w echo --bin ./bin/echo --node-count 1 --time-limit 10

test-2: 2_unique_ids maelstrom
	./maelstrom/maelstrom test -w unique-ids --bin ./bin/unique_ids --time-limit 30 --rate 1000 --node-count 3 --availability total --nemesis partition

test-3a: 3_broadcast maelstrom
	./maelstrom/maelstrom test -w broadcast --bin ./bin/broadcast --node-count 1 --time-limit 20 --rate 10

test-3b: 3_broadcast maelstrom
	./maelstrom/maelstrom test -w broadcast --bin ./bin/broadcast --node-count 5 --time-limit 20 --rate 10

test-3c: 3_broadcast maelstrom
	./maelstrom/maelstrom test -w broadcast --bin ./bin/broadcast --node-count 5 --time-limit 20 --rate 10 --nemesis partition

test-3d: 3_broadcast maelstrom
	./maelstrom/maelstrom test -w broadcast --bin ./bin/broadcast --node-count 25 --time-limit 20 --rate 100 --latency 100

results: maelstrom
	./maelstrom/maelstrom serve

clean:
	@rm -rf $(BIN_DIR) maelstrom

.PHONY: all clean $(PROJECTS)
