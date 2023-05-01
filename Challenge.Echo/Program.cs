using Challenge.Echo;
using Maelstrom;

var messageId = 0;
var node = new Node();

node.On("echo", message =>
{
    var body = message.GetBody<EchoBody>();
    node.Reply(message, new EchoBody
    {
        Type = "echo_ok", MessageId = Interlocked.Increment(ref messageId), Echo = body.Echo
    });
    return Task.CompletedTask;
});

node.Run();