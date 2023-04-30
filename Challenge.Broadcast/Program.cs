using System.Collections.Concurrent;
using Challenge.Broadcast;
using Maelstrom;

var neighbors = new List<string>();
var messages = new ConcurrentDictionary<int, byte>();

var node = new Node();

void HandleBroadcast(Message message)
{
    var body = message.GetBody<BroadcastBody>();
    var messageValue = body.Message;
    node.Log("Received message " + messageValue);

    if (message.Source.StartsWith("c"))
    {
        node.Reply(message, new Body { Type = "broadcast_ok" });
    }

    if (!messages.TryAdd(body.Message, 0x0))
    {
        node.Log("Message " + messageValue + " already received, ignoring");
        return;
    }

    foreach (var neighbor in neighbors.Where(neighbor => !neighbor.Equals(message.Source)))
    {
        node.Rpc(neighbor, new BroadcastBody { Type = "broadcast", Message = body.Message },
            msg => node.Log($"{neighbor} replied {msg.Body["type"]} for {body.Message}"));
    }
}

void HandleTopology(Message message)
{
    var body = message.GetBody<TopologyBody>();
    neighbors = body.Topology[node.Id];
    node.Log("Neighbors: [" + string.Join(",", neighbors) + "]");
    node.Reply(message, new Body { Type = "topology_ok" });
}

void HandleRead(Message message)
{
    var response = new ReadResponseBody { Type = "read_ok", Messages = messages.Keys.ToList() };
    node.Log("read [" + string.Join(",", response.Messages) + "]");
    node.Reply(message, response);
}

node.On("broadcast", HandleBroadcast)
    .On("topology", HandleTopology)
    .On("read", HandleRead)
    .Run();