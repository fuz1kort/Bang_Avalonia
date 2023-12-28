using Band_Server;

Console.Title = "BangServer";

var server = new BangServer();
await server.StartAsync();
server.AcceptClients();
server.StartGame();