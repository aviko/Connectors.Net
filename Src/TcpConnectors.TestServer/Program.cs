using System;
using System.Collections.Generic;

namespace TcpConnectors.TestServer
{
    class Program
    {
        private static ServerConnectors _serverConnectors;

        static void Main(string[] args)
        {
            Console.WriteLine("TcpConnectors.TestServer");

            _serverConnectors = new ServerConnectors(GetTypeMap());
            _serverConnectors.OnNewConnector += ServerConnectors_OnNewConnector;
            _serverConnectors.OnPacket += ServerConnectors_OnPacket; ;


            _serverConnectors.Listen(1111);

            Console.WriteLine("Press Enter to continue...");
            Console.ReadLine();

        }

        private static void ServerConnectors_OnPacket(ServerConnectorContext serverConnectorContext, int module, int command, object packet)
        {
            Console.WriteLine($"ServerConnectors_OnPacket RemoteEndPoint:{serverConnectorContext.Socket.RemoteEndPoint.ToString()} module:{module}  command:{command} packet:{packet}");

            _serverConnectors.Send(x => true, module, command, packet);


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
