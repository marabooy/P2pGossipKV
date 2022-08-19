using Akka.Actor;
using Akka.Event;
using ConsoleApp1.Models;

namespace ConsoleApp1;

internal partial class P2PNode : UntypedActor
{
    protected ILoggingAdapter Log { get; } = Context.GetLogger();

    public P2PNode(string nodeId, IEnumerable<char> keySpace, string[] adjacent)
    {
        this.nodeId = nodeId;
        this.keySpace = new HashSet<string>(keySpace.Select(x => x.ToString()), StringComparer.OrdinalIgnoreCase);
        this.Log.Info($"starting node {nodeId} with Keyspace [{string.Join(",", keySpace)}]");
        this.adjacent = adjacent;
        foreach (var key in adjacent)
        {
            this.AlivePeers[key] = true;
        }
    }

    protected override void OnReceive(object message)
    {
        if (message is not Ping && message is not Pong && message is not Reconnect && message is not Disconnect)
        {
            Log.Info($"{nodeId} recieved a message {message} from {GetSender()}");
        }
        this.OnMessage(message);
    }

    protected void SendToPeer(string peerId, object message)
    {

        if (peerId.Equals(nodeId) || peerId.Equals("supervisor"))
        {
            this.OnMessage(message);
            return;
        }

        if (!(this.AlivePeers.GetValueOrDefault(peerId) == true))
        {
            this.Log.Warning($"Message dropped for {peerId} from {nodeId} since edge is unresponsive!");
            return;
        }

        Task.Delay(2000).Wait();
        var selection = Context.ActorSelection($"../{peerId}");
        selection.Tell(message);
        if (message is not Ping && message is not Pong && message is not Reconnect && message is not Disconnect)
        {
            Log.Info($"{nodeId} sending a message {message} to {peerId}");
        }
    }

    private string GetSender()
    {
        return Sender.Path.Elements[Sender.Path.Elements.Count - 1];
    }

    protected override void PreStart()
    {
        var ping = new Ping();
        this.BroadCastToPeers(ping, nodeId);
    }

    public static Props Props(string nodeId, IEnumerable<char> keySpace, params string[] adjacent) => Akka.Actor.Props.Create<P2PNode>(() => new P2PNode(nodeId, keySpace, adjacent));
}
