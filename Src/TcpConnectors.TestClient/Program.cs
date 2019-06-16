using System;

namespace TcpConnectors.TestClient
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("TcpConnectors.TestClient");

            var clientConnector = new ClientConnector();

            clientConnector.Connect("127.0.0.1", 1111);

            clientConnector.Send(1, 1, "hello !");

            Console.WriteLine("Press Enter to continue...");
            Console.ReadLine();
        }
    }
}
