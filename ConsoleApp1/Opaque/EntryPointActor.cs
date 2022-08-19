using Akka.Actor;

namespace ConsoleApp1;

internal partial class EntryPointActor : UntypedActor
{
    protected override void OnReceive(object message)
    {
       this.OnMessage(message);
    }

    IActorRef GetOrCreate(string childName, IEnumerable<char> keySpace, params string[] adjacent)
    {
        var child = Context.Child(childName);
        if (Equals(child, ActorRefs.Nobody))
            child = Context.ActorOf(P2PNode.Props(childName, keySpace, adjacent), childName);
        return child;
    }

    public static Props Props() => Akka.Actor.Props.Create<EntryPointActor>();
}
