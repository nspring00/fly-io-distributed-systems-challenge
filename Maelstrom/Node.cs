using System.Collections.Concurrent;
using System.Text.Json.Nodes;

namespace Maelstrom;

public class Node
{
    public string Id { get; private set; } = "<uninitialized>";
    public IList<string> NodeIds { get; } = new List<string>();
    
    private readonly IDictionary<string, Action<Message>> _requestHandlers = new Dictionary<string, Action<Message>>();
    private readonly ConcurrentDictionary<int, Action<Message>> _callbackHandlers = new ConcurrentDictionary<int, Action<Message>>();
    
    private int _lastMessageId;
    
    public Node On(string type, Action<Message> handler)
    {
        if (!_requestHandlers.TryAdd(type, handler))
        {
            throw new Exception("Handler for type " + type + " already registered");
        }
        return this;
    }
    
    private void Send(Message message)
    {
        Console.Out.WriteLine(message.ToJson());
        Console.Out.Flush();
    }
    
    public void Send(string dest, JsonObject body)
    {
        var message = new Message(Id, dest, body);
        Send(message);
    }
    
    public void Reply(Message request, JsonObject body)
    {
        body.Add("in_reply_to", request.Body["msg_id"]?.GetValue<int>() ?? -1);
        Send(request.Src, body);
    }
    
    public void Rpc(string dest, JsonObject body, Action<Message> callback)
    {
        var msgId = Interlocked.Increment(ref _lastMessageId);
        if (!_callbackHandlers.TryAdd(msgId, callback))
        {
            throw new Exception("Failed to add callback for message id " + msgId);
        }
        body.Add("msg_id", msgId);
        Send(dest, body);
    }
    
    public void HandleRequest(string type, Message request)
    {
        if (_requestHandlers.ContainsKey(type))
        {
            ThreadPool.QueueUserWorkItem(_ => _requestHandlers[type](request));
            return;
        }

        if ("init".Equals(type))
        {
            return;
        }

        Console.Error.WriteLine("No handler found for request type " + type);
    }
    
    public void HandleInit(Message message)
    {
        Id = message.Body["node_id"]?.GetValue<string>() ?? throw new Exception("No node id found in init");
        foreach (var nodeId in message.Body["node_ids"]?.AsArray() ?? throw new Exception("No node ids found in init"))
        {
            NodeIds.Add(nodeId!.GetValue<string>());
        }
        Console.Error.WriteLine("Initialized node " + Id + " with " + NodeIds.Count + " nodes");
    }

    public void HandleMessage(Message message)
    {
        Console.Error.WriteLine("Received: " + message);

        var type = message.Body["type"]?.GetValue<string>();
        var replyTo = message.Body["in_reply_to"]?.GetValue<int>();

        if (replyTo is not null)
        {
            if (_callbackHandlers.TryRemove(replyTo.Value, out var handler))
            {
                ThreadPool.QueueUserWorkItem(_ => handler(message));
                return;
            }

            Console.Error.WriteLine("No callback found for message id " + replyTo);
        }
        
        switch (type)
        {
            case null:
                Console.Error.WriteLine("No type found in message");
                return;
            case "init":
            {
                HandleInit(message);
                HandleRequest(type, message);
                var response = new JsonObject
                {
                    {
                        "type", "init_ok"
                    }
                };
                Reply(message, response);
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
                var message = Message.FromJson(line);
                HandleMessage(message);
            }
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e.Message);
        }
    }
}