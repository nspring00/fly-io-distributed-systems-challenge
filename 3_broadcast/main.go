package main

import (
	"encoding/json"
	"fmt"
	maelstrom "github.com/jepsen-io/maelstrom/demo/go"
	"log"
)

func main() {
	n := maelstrom.NewNode()

	var seen = map[int]struct{}{}
	var seenList []int
	var topology map[string][]string

	n.Handle("topology", func(msg maelstrom.Message) error {
		var body TopologyMessageBody
		if err := json.Unmarshal(msg.Body, &body); err != nil {
			return fmt.Errorf("unmarshal topology message body: %w", err)
		}

		topology = body.Topology
		_ = topology

		return n.Reply(msg, createMessage("topology_ok"))
	})

	n.Handle("broadcast", func(msg maelstrom.Message) error {
		var body BroadcastMessageBody
		if err := json.Unmarshal(msg.Body, &body); err != nil {
			return fmt.Errorf("unmarshal broadcast message body: %w", err)
		}

		_, known := seen[body.Message]
		if !known {
			seen[body.Message] = struct{}{}
			seenList = append(seenList, body.Message)
		}

		return n.Reply(msg, createMessage("broadcast_ok"))
	})

	n.Handle("read", func(msg maelstrom.Message) error {
		reply := createMessage("read_ok")
		reply["messages"] = seenList
		return n.Reply(msg, reply)
	})

	if err := n.Run(); err != nil {
		log.Fatal(err)
	}
}

func createMessage(messageType string) map[string]any {
	return map[string]any{
		"type": messageType,
	}
}

type BroadcastMessageBody struct {
	maelstrom.MessageBody
	Message int `json:"message"`
}

type TopologyMessageBody struct {
	maelstrom.MessageBody
	Topology map[string][]string `json:"topology"`
}
