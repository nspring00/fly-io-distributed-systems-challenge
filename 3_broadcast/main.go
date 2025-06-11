package main

import (
	"encoding/json"
	"fmt"
	maelstrom "github.com/jepsen-io/maelstrom/demo/go"
	"log"
	"sync"
)

func main() {
	n := maelstrom.NewNode()
	var mu sync.Mutex

	var seen = map[int]struct{}{}
	var seenList []int
	var neighbors []string

	n.Handle("topology", func(msg maelstrom.Message) error {
		var body TopologyMessageBody
		if err := json.Unmarshal(msg.Body, &body); err != nil {
			return fmt.Errorf("unmarshal topology message body: %w", err)
		}

		mu.Lock()
		neighbors = body.Topology[n.ID()]
		mu.Unlock()

		return n.Reply(msg, createMessage("topology_ok"))
	})

	n.Handle("broadcast", func(msg maelstrom.Message) error {
		var body BroadcastMessageBody
		if err := json.Unmarshal(msg.Body, &body); err != nil {
			return fmt.Errorf("unmarshal broadcast message body: %w", err)
		}

		mu.Lock()
		_, known := seen[body.Message]
		if !known {
			seen[body.Message] = struct{}{}
			seenList = append(seenList, body.Message)
			mu.Unlock()

			// Forward to neighbors
			for _, neighbor := range neighbors {
				if err := n.Send(neighbor, msg.Body); err != nil {
					return fmt.Errorf("send broadcast message: %w", err)
				}
			}
		} else {
			mu.Unlock()
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
