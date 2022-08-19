// See https://aka.ms/new-console-template for more information
using Akka.Actor;
using ConsoleApp1;
using ConsoleApp1.Models;
using ConsoleApp1.Simple;

var system = ActorSystem.Create("example");

var echo = system.ActorOf<GreetingsActor>("sheng");

# region Menu.
var supervisor = system.ActorOf<EntryPointActor>("supervisor");
void PrintMenu()
{
    Console.WriteLine("A simple P2P gossip protocol with a distributed key value store!");
    Console.WriteLine("Commands");
    Console.WriteLine(" push [Origin] [key] [value] !");
    Console.WriteLine(" query [Origin] [key]!");
    Console.WriteLine(" disconnect [Peer1] [Peer2]!");
    Console.WriteLine(" reconnect [Peer1] [Peer2]!");
    Console.WriteLine(" start");
    Console.WriteLine(" clear # to clear screen");
    Console.WriteLine(" exit # to exit");
}
# endregion

while (true)
{
    await Task.Delay(5);
    Console.Write("Input />");
    var command = Console.ReadLine();
    if (command == "exit")
    {
        Console.WriteLine("Exiting");
        await system.Terminate();
        break;
    }

    var parameters = command.Split(" ");
    Guid myuuid = Guid.NewGuid();

    switch (parameters[0])
    {
        #region Demo P2P
        case "query":
            var cmd = new Command(myuuid.ToString(), parameters[0], parameters[1], parameters[2]);
            supervisor.Tell(cmd);
            break;
        case "push":
            var pushCmd = new Command(myuuid.ToString(), parameters[0], parameters[1], parameters[2], parameters[3]);
            supervisor.Tell(pushCmd);
            break;
        case "disconnect":
            var disconnection = new Disconnect(parameters[1], parameters[2]);
            supervisor.Tell(disconnection);
            break;
        case "reconnect":
            var reconnection = new Reconnect(parameters[1], parameters[2]);
            supervisor.Tell(reconnection);
            break;
        case "start":
            supervisor.Tell("start");
            break;
        case "clear":
        case "print-menu":
            Console.Clear();
            PrintMenu();
            break;
        #endregion
        default:
            var formated = await echo.Ask(command);
            Console.WriteLine($"{formated}");
            break;
    }
}