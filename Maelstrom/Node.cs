using System.Collections.Concurrent;
using System.Text.Json;

namespace Maelstrom;

public class Node
{
    public string Id { get; private set; } = "<uninitialized>";
    public IList<string> NodeIds { get; } = new List<string>();
    
    private readonly Dictionary<string, Func<Message, Task>> _requestHandlers = new();
    private readonly ConcurrentDictionary<int, Func<Message, Task>> _callbackHandlers = new();
    
    private int _lastMessageId;
    
    public void Log(string message)
    {
        Console.Error.WriteLine(message);
    }
    
    public Node On(string type, Func<Message, Task> handler)
    {
        if (!_requestHandlers.TryAdd(type, handler))
        {
            throw new Exception("Handler for type " + type + " already registered");
        }
        return this;
    }
    
    private void Send(Message message)
    {
        var line = JsonSerializer.Serialize(message);
        Log("Sent: " + line);
        Console.Out.WriteLine(line);
        Console.Out.Flush();
    }
    
    public void Send<TBody>(string dest, TBody body) where TBody : Body
    {
        Send(new Message
        {
            Source = Id, Destination = dest, Body = JsonSerializer.SerializeToNode(body)!
        });
    }
    
    public void Reply<TBody>(Message request, TBody body) where TBody : Body
    {
        body.InReplyTo = request.Body["msg_id"]?.GetValue<int>() ?? -1;
        Send(request.Source, body);
    }
    
    public void Rpc<TBody>(string dest, TBody body, Func<Message, Task> callback) where TBody : Body
    {
        var msgId = Interlocked.Increment(ref _lastMessageId);
        if (!_callbackHandlers.TryAdd(msgId, callback))
        {
            throw new Exception("Failed to add callback for message id " + msgId);
        }
        body.MessageId = msgId;
        Send(dest, body);
    }
    
    public void HandleRequest(string type, Message request)
    {
        if (_requestHandlers.TryGetValue(type, out var handler))
        {
            _ = handler(request);
            return;
        }

        if ("init".Equals(type))
        {
            return;
        }

        Log("No handler found for request type " + type);
    }
    
    public void HandleInit(Message message)
    {
        var body = message.GetBody<InitBody>();
        Id = body.Id;
        NodeIds.Clear();
        foreach (var nodeId in body.NodeIds)
        {
            NodeIds.Add(nodeId);
        }
        Log("Initialized node " + Id + " with " + NodeIds.Count + " nodes");
    }

    public void HandleMessage(Message message)
    {
        var type = message.Body["type"]?.GetValue<string>();
        var replyTo = message.Body["in_reply_to"]?.GetValue<int>();

        if (replyTo is not null)
        {
            if (_callbackHandlers.TryRemove(replyTo.Value, out var handler))
            {
                _ = handler(message);
                return;
            }

            Log("No callback found for message id " + replyTo);
        }
        
        switch (type)
        {
            case null:
                Log("No type found in message");
                return;
            case "init":
            {
                HandleInit(message);
                HandleRequest(type, message);
                Reply(message, new Body {Type = "init_ok"});
                return;
            }
            default:
                HandleRequest(type, message);
                break;
        }
    }
    

    public void Run()
    {
        try
        {
            while (true)
            {
                var line = Console.ReadLine()!;
                Log("Received: " + line);
                var message = JsonSerializer.Deserialize<Message>(line) ?? throw new Exception("Failed to deserialize message");
                HandleMessage(message);
            }
        }
        catch (Exception e)
        {
            Log(e.Message);
        }
    }
}