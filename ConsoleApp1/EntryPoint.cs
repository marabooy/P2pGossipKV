using ConsoleApp1.Models;

namespace ConsoleApp1;

internal partial class EntryPointActor
{
    #region Jagged
    readonly Dictionary<string, string[]> jaggedTopology = new()
    {
        { "A", new string[]{ "B","C" } },
        { "B", new string[]{ "D","E", "A" } },
        { "C", new string[]{ "A" } },
        { "D", new string[]{ "E","F","B" } },
        { "E", new string[]{ "D", "B"} },
        { "F", new string[]{ "D" } },
    };
    #endregion

    // A - B - C - D - E
    readonly Dictionary<string, string[]> linearTopology = new()
    {
        { "A", new string[] { "B" } },
        { "B" , new string[] { "A" , "C" } },
        { "C", new string[] { "B" , "D" } },
        { "D", new string[] { "C" , "E" } },
        { "E" , new string[] {"D" }}
    };

    public void OnMessage(object message)
    {
        switch (message)
        {
            case "start":
                var chunks = GetAlphabet().Chunk((26 / jaggedTopology.Count) + 1).ToArray();
                var i = 0;
                foreach (var (child, adj) in jaggedTopology)
                {
                    _ = this.GetOrCreate(child, chunks[i++], adj);
                }
                break;
            case Command command:
                var origin = Context.ActorSelection($"{command.Origin}");
                origin.Tell(command);
                break;

            case Reconnect reconnect:
                foreach (var peer in reconnect.Peers)
                {
                    var peerRef = Context.ActorSelection($"{peer}");
                    peerRef.Tell(reconnect);
                }
                break;
            case Disconnect disconnect:
                foreach (var peer in disconnect.Peers)
                {
                    var peerRef = Context.ActorSelection($"{peer}");
                    peerRef.Tell(disconnect);
                }
                break;
        }
    }

    #region KeySpace
    public IEnumerable<char> GetAlphabet()
    {
        for (char c = 'A'; c <= 'Z'; c++)
        {
            yield return c;
        }
    }
    #endregion
}
