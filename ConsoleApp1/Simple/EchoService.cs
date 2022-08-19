using Akka.Actor;
using Akka.Event;

namespace ConsoleApp1.Simple;

internal class GreetingsActor : UntypedActor
{
    readonly string[] messages = new string[]
    {
        "Greetings!",
        "Welcome",
        "(-(-_(-_-)_-)-)"
    };
    readonly Random random = new();

    protected ILoggingAdapter Log { get; } = Context.GetLogger();

    protected override void OnReceive(object message)
    {
        switch (message)
        {
            case "hello":
                Sender.Tell("General Kenobi");
                break;
            default:
                Sender.Tell(messages[random.Next(0, messages.Length)]);
                break;
        }
    }
}
