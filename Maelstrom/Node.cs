using System.Text.Json.Nodes;

namespace Maelstrom;

public class Node
{
    public string NodeId { get; private set; } = "<uninitialized>";
    private readonly IList<string> _nodeIds = new List<string>();
    
    private readonly IDictionary<string, Action<Message>> _requestHandlers = new Dictionary<string, Action<Message>>();

    public Node On(string type, Action<Message> handler)
    {
        _requestHandlers[type] = handler;
        return this;
    }
    
    public void Send(Message message)
    {
        Console.WriteLine(message.ToJson());
    }
    
    public void Send(string dest, JsonObject body)
    {
        var message = new Message(NodeId, dest, body);
        Send(message);
    }
    
    public void Reply(Message request, JsonObject body)
    {
        body.Add("in_reply_to", request.Body["msg_id"]?.GetValue<int>() ?? -1);
        Send(request.Src, body);
    }
    
    public void HandleRequest(Message request)
    {
        var type = request.Body["type"]?.GetValue<string>() ?? throw new Exception("No type found in request");
        if (_requestHandlers.ContainsKey(type))
        {
            _requestHandlers[type](request);
            return;
        }

        if ("init".Equals(type))
        {
            return;
        }

        throw new Exception("No handler found for request type " + type);
    }
    
    public void HandleInit(Message message)
    {
        NodeId = message.Body["node_id"]?.GetValue<string>() ?? throw new Exception("No node id found in init");
        foreach (var nodeId in message.Body["node_ids"]?.AsArray() ?? throw new Exception("No node ids found in init"))
        {
            _nodeIds.Add(nodeId!.GetValue<string>());
        }
        Console.Error.WriteLine("Initialized node " + NodeId + " with " + _nodeIds.Count + " nodes");
    }

    public void HandleMessage(Message message)
    {
        var type = message.Body["type"]?.GetValue<string>();

        if ("init".Equals(type))
        {
            HandleInit(message);
            HandleRequest(message);
            var response = new JsonObject
            {
                {
                    "type", "init_ok"
                }
            };
            Reply(message, response);
            return;
        }

        HandleRequest(message);
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