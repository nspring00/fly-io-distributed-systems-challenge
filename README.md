![CI Tests Badge](https://github.com/nspring00/fly-io-distributed-systems-challenge/actions/workflows/main.yml/badge.svg?branch=main)

# Fly.io Distributed Systems Challenges

Further information on the challenges can be found on the [official website by Fly.io](https://fly.io/dist-sys/).

## Challenges
- [x] [#1: Echo](https://fly.io/dist-sys/1/)
- [x] [#2: Unique ID Generation](https://fly.io/dist-sys/2/)
- [ ] #3: Broadcast
  - [x] [#3a: Single-Node Broadcast](https://fly.io/dist-sys/3a/)
  - [ ] [#3b: Multi-Node Broadcast](https://fly.io/dist-sys/3b/)
  - [ ] [#3c: Fault Tolerant Broadcast](https://fly.io/dist-sys/3c/)
  - [ ] [#3d: Efficient Broadcast, Part I](https://fly.io/dist-sys/3d/)
  - [ ] [#3e: Efficient Broadcast, Part II](https://fly.io/dist-sys/3e/)
- [ ] [#4: Grow-Only Counter](https://fly.io/dist-sys/4/)
- [ ] #5: Kafka-Style Log
  - [ ] [#5a: Single-Node Kafka-Style Log](https://fly.io/dist-sys/5a/)
  - [ ] [#5b: Multi-Node Kafka-Style Log](https://fly.io/dist-sys/5b/)
  - [ ] [#5c: Efficient Kafka-Style Log](https://fly.io/dist-sys/5c/)
- [ ] #6: Totally-Available Transactions
  - [ ] [#6a: Single-Node, Totally-Available Transactions](https://fly.io/dist-sys/6a/)
  - [ ] [#6b: Totally-Available, Read Uncommitted Transactions](https://fly.io/dist-sys/6b/)
  - [ ] [#6b: Totally-Available, Read Committed Transactions](https://fly.io/dist-sys/6c/)


## How to run

`java -jar lib\maelstrom.jar test -w echo --bin ..\fly-io-distributed-systems-challenge\Challenge.Echo\bin\Debug\net7.0\Challenge.Echo.exe --node-count 1 --time-limit 10`