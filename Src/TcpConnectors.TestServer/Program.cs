using System;
using System.Collections.Generic;

namespace TcpConnectors.TestServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("TcpConnectors.TestServer");

            var serverConnectors = new ServerConnectors(GetTypeMap());
            serverConnectors.OnNewConnector += ServerConnectors_OnNewConnector;


            serverConnectors.Listen(1111);

            Console.WriteLine("Press Enter to continue...");
            Console.ReadLine();

        }

        private static void ServerConnectors_OnNewConnector(ServerConnectorContext serverConnectorContext)
        {
            Console.WriteLine($"ServerConnectors_OnNewConnector RemoteEndPoint:{serverConnectorContext.Socket.RemoteEndPoint.ToString()}");
        }

        static Dictionary<Tuple<int, int>, Type> GetTypeMap()
        {
            return new Dictionary<Tuple<int, int>, Type>()
            {
               { new Tuple<int,int>(1,1) , typeof(string) }
            };
        }
    }
}
