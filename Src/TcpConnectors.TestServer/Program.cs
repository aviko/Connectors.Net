using System;

namespace TcpConnectors.TestServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("TcpConnectors.TestServer");

            var serverConnectors = new ServerConnectors();
            serverConnectors.OnNewConnector += ServerConnectors_OnNewConnector;


            serverConnectors.Listen(1111);

            Console.WriteLine("Press Enter to continue...");
            Console.ReadLine();

        }

        private static void ServerConnectors_OnNewConnector(ServerConnectorContext serverConnectorContext)
        {
            Console.WriteLine($"ServerConnectors_OnNewConnector RemoteEndPoint:{serverConnectorContext.Socket.RemoteEndPoint.ToString()}");
        }
    }
}
