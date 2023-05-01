using Challenge.UniqueIds;
using Maelstrom;

var node = new Node();

var messageCounter = 1;
const string messageIdTemplate = "{0}-{1}";

string GenerateId()
{
    return string.Format(messageIdTemplate, node.Id, Interlocked.Increment(ref messageCounter));
}

node.On("generate", message =>
{
    node.Reply(message, new UniqueIdsBody
    {
        Type = "generate_ok",
        Id = GenerateId()
    });
    return Task.CompletedTask;
});

node.Run();