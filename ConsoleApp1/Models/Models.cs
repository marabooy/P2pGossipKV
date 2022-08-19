namespace ConsoleApp1.Models;

record class Ping();

record Pong();

record Disconnect(params string[] Peers);

record Reconnect(params string[] Peers);

record Query(string Id, string Key);

record QueryHit(string Id, string PreviousId, string Query, string Value);

record QueryMiss(string Id, string PreviousId, string Query);

record Push(string Id, string Key, string Value);

record Command(string Id, string Name, string Origin, params string[] Values);