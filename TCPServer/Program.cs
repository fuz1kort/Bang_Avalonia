namespace TCPServer
{
    internal class Program
    {
        private static void Main()
        {
            Console.Title = "XServer";

            var server = new XServer();
            server.Start();
            server.AcceptClients();
        }
    }
}