package main

import (
	"context"
	"encoding/json"
	"fmt"
	maelstrom "github.com/jepsen-io/maelstrom/demo/go"
	"log"
	"sync"
	"time"
)

func main() {
	n := maelstrom.NewNode()
	var mu sync.Mutex

	var seen = map[int]struct{}{}
	var seenList = make([]int, 0)
	var neighbors = make([]string, 0)

	bgCtx, bgCancel := context.WithCancel(context.Background())
	defer bgCancel()

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

	saveMessage := func(msg int) bool {
		_, known := seen[msg]
		if !known {
			seen[msg] = struct{}{}
			seenList = append(seenList, msg)
		}
		return known
	}

	n.Handle("broadcast", func(msg maelstrom.Message) error {
		var body BroadcastMessageBody
		if err := json.Unmarshal(msg.Body, &body); err != nil {
			return fmt.Errorf("unmarshal broadcast message body: %w", err)
		}

		mu.Lock()
		saveMessage(body.Message)
		mu.Unlock()

		return n.Reply(msg, createMessage("broadcast_ok"))
	})

	n.Handle("broadcast_list", func(msg maelstrom.Message) error {
		var body BroadcastListMessageBody
		if err := json.Unmarshal(msg.Body, &body); err != nil {
			return fmt.Errorf("unmarshal broadcast list message body: %w", err)
		}

		mu.Lock()
		for _, message := range body.Messages {
			saveMessage(message)
		}
		mu.Unlock()

		return nil
	})

	n.Handle("read", func(msg maelstrom.Message) error {
		reply := createMessage("read_ok")
		reply["messages"] = seenList
		return n.Reply(msg, reply)
	})

	// Background sender thread
	go func() {
		ticker := time.NewTicker(500 * time.Millisecond)
		defer ticker.Stop()

		for {
			select {
			case <-bgCtx.Done():
				return
			case <-ticker.C:
				mu.Lock()
				for _, neighbor := range neighbors {
					msg := createMessage("broadcast_list")
					msg["messages"] = seenList
					if err := n.Send(neighbor, msg); err != nil {
						log.Fatal(err)
					}
				}
				mu.Unlock()
			}
		}
	}()

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

type BroadcastListMessageBody struct {
	maelstrom.MessageBody
	Messages []int `json:"messages"`
}
