using Akka;
using ConsoleApp1.Models;
using System;

namespace ConsoleApp1;

internal partial class P2PNode
{
    readonly Dictionary<string, bool> AlivePeers = new();

    readonly Dictionary<string, string> keyValuePairs = new(StringComparer.OrdinalIgnoreCase);

    readonly Dictionary<string, string> seenMessages = new();

    private readonly string nodeId;
    private readonly string[] adjacent;
    private readonly HashSet<string> keySpace;

    protected void OnMessage(object message)
    {
        var senderAddress = this.GetSender();

        switch (message)
        {
            case Ping:
                this.SendToPeer(senderAddress, new Pong());
                Sender.Tell(new Pong(), Self);
                break;

            case Pong:
                AlivePeers[senderAddress] = true;
                break;
            case Query query:
                if (seenMessages.ContainsKey(query.Id))
                {
                    break;
                }

                seenMessages.Add(query.Id, senderAddress);
                HandleQuery(senderAddress, query);
                break;
            case QueryHit queryHit:
                var origin = this.seenMessages.GetValueOrDefault(queryHit.PreviousId);
                if (origin != null)
                {
                    this.SendToPeer(origin, queryHit);
                }
                else
                {
                    this.Log.Info($"Recieved a response for my query {queryHit.Value}");
                }

                break;
            case QueryMiss queryMiss:

                var prev = this.seenMessages.GetValueOrDefault(queryMiss.PreviousId);
                if (prev != null)
                {
                    this.SendToPeer(prev, queryMiss);
                }
                else
                {
                    this.Log.Info($"Recieved a miss response for my query {queryMiss.Query}");
                }

                break;
            case Push push:
                if (seenMessages.ContainsKey(push.Id))
                {
                    break;
                }

                seenMessages.Add(push.Id, senderAddress);

                HandlePush(senderAddress, push);
                break;
            case Disconnect disconnect:
                foreach (var peer in disconnect.Peers)
                {
                    if (AlivePeers.ContainsKey(peer))
                    {
                        AlivePeers[peer] = false;
                    }
                }
                break;
            case Reconnect reconnect:
                foreach (var peer in reconnect.Peers)
                {
                    if (AlivePeers.ContainsKey(peer))
                    {
                        AlivePeers[peer] = true;
                    }
                }
                break;
            case Command command:

                if (command.Name.Equals("query"))
                {
                    var query = new Query(command.Id, command.Values[0]);
                    HandleQuery(senderAddress, query);
                }

                if (command.Name.Equals("push"))
                {
                    var push = new Push(command.Id, command.Values[0], command.Values[1]);
                    this.HandlePush(senderAddress, push);
                }

                break;
        }
    }

    private void HandleQuery(string senderAddress, Query query)
    {
        var key = query.Key[0];
        if (!keySpace.Contains(key.ToString()))
        {
            this.BroadCastToPeers(query, senderAddress);
            return;
        }
        Guid guid = Guid.NewGuid();

        var response = keyValuePairs.GetValueOrDefault(query.Key);
        if (response != null)
        {

            var queryHit = new QueryHit(guid.ToString(), query.Id, query.Key, response);
            this.SendToPeer(senderAddress, queryHit);
        }
        else
        {
            var miss = new QueryMiss(guid.ToString(), query.Id, query.Key);
            this.SendToPeer(senderAddress, miss);
        }
    }

    private void HandlePush(string senderAddress, Push push)
    {
        var key = push.Key[0];
        if (keySpace.Contains(key.ToString()))
        {
            this.keyValuePairs[push.Key] = push.Value;
        }
        else
        {
            this.BroadCastToPeers(push, senderAddress);
        }
    }

    private void BroadCastToPeers(object query, string excludedPeer)
    {
        foreach (var peer in this.adjacent.Where(ad => !ad.Equals(excludedPeer)))
        {
            this.SendToPeer(peer, query);
        }
    }
}
