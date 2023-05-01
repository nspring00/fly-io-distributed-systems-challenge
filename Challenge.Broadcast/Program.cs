using System.Collections.Concurrent;
using Challenge.Broadcast;
using Maelstrom;

var neighbors = new List<string>();
var messages = new ConcurrentDictionary<int, byte>();

var node = new Node();

async Task HandleBroadcast(Message message)
{
    var body = message.GetBody<BroadcastBody>();
    var messageValue = body.Message;
    node.Log("Received message " + messageValue);

    node.Reply(message, new Body { Type = "broadcast_ok" });
    node.Log("Replied to message " + messageValue + " with broadcast_ok");

    if (!messages.TryAdd(body.Message, 0x0))
    {
        node.Log("Message " + messageValue + " already received, ignoring");
        return;
    }

    var unacked = new ConcurrentDictionary<string, byte>(neighbors.Select(x => new KeyValuePair<string, byte>(x, 0x0)));
    unacked.TryRemove(message.Source, out _);
    node.Log("Broadcasting message " + messageValue + " to " + string.Join(",", unacked));
    while (!unacked.IsEmpty) 
    {
        foreach (var neighbor in unacked.Keys)
        {
            node.Rpc(neighbor, new BroadcastBody { Type = "broadcast", Message = body.Message },
            msg =>
            {
                node.Log("Message " + messageValue + " acked by " + neighbor + ", removing from unacked");
                unacked.TryRemove(neighbor, out _);
                return Task.CompletedTask;
            });
        }
        await Task.Delay(1000);
        if (!unacked.IsEmpty)
        {
            node.Log("Message " + messageValue + " not acked by " + string.Join(",", unacked) + ", retrying");
        }
    }
    node.Log("Message " + messageValue + " broadcasted to all neighbors");
}

Task HandleTopology(Message message)
{
    var body = message.GetBody<TopologyBody>();
    neighbors = body.Topology[node.Id];
    node.Log("Neighbors: [" + string.Join(",", neighbors) + "]");
    node.Reply(message, new Body { Type = "topology_ok" });
    return Task.CompletedTask;
}

Task HandleRead(Message message)
{
    var response = new ReadResponseBody { Type = "read_ok", Messages = messages.Keys.ToList() };
    node.Log("read [" + string.Join(",", response.Messages) + "]");
    node.Reply(message, response);
    return Task.CompletedTask;
}

node.On("broadcast", HandleBroadcast)
    .On("topology", HandleTopology)
    .On("read", HandleRead)
    .Run();