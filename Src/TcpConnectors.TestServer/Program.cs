using System;
using System.Collections.Generic;
using TcpConnectors.TestCommon;

namespace TcpConnectors.TestServer
{
    class Program
    {
        private static ServerConnectors _serverConnectors;

        static void Main(string[] args)
        {
            Console.WriteLine("TcpConnectors.TestServer");

            _serverConnectors = new ServerConnectors(PacketsUtils.GetClient2ServerMapping());
            _serverConnectors.OnNewConnector += ServerConnectors_OnNewConnector;
            _serverConnectors.OnPacket += ServerConnectors_OnPacket; ;
            _serverConnectors.OnRequestPacket += ServerConnectors_OnRequestPacket;
            _serverConnectors.OnDisconnect += ServerConnectors_OnDisconnect;
            _serverConnectors.OnException += ServerConnectors_OnException;


            _serverConnectors.Listen(1111);

            Console.WriteLine("Press Enter to continue...");
            Console.ReadLine();

        }

        private static void ServerConnectors_OnException(ServerConnectorContext connectorContext, Exception exp)
        {
            var remoteEndPoint = "NA";
            try { remoteEndPoint = connectorContext.Socket.RemoteEndPoint.ToString(); } catch { }

            Console.WriteLine($"ServerConnectors_OnException RemoteEndPoint:{remoteEndPoint} ex:{exp.ToString()}");
        }

        private static void ServerConnectors_OnDisconnect(ServerConnectorContext serverConnectorContext)
        {
            var remoteEndPoint = "NA";
            try { remoteEndPoint = serverConnectorContext.Socket.RemoteEndPoint.ToString(); } catch { }

            Console.WriteLine($"ServerConnectors_OnDisconnect RemoteEndPoint:{remoteEndPoint}");
        }

        private static object ServerConnectors_OnRequestPacket(ServerConnectorContext serverConnectorContext, int module, int command, object packet)
        {
            Console.WriteLine($"ServerConnectors_OnRequestPacket RemoteEndPoint:{serverConnectorContext.Socket.RemoteEndPoint.ToString()} module:{module}  command:{command} packet:{packet}");
            if (module == LoginResponsePacket.MODULE && command == LoginResponsePacket.COMMAND)
            {
                return new LoginResponsePacket() { Username = "!" };
            }
            return null;
        }

        private static void ServerConnectors_OnPacket(ServerConnectorContext serverConnectorContext, int module, int command, object packet)
        {
            Console.WriteLine($"ServerConnectors_OnPacket RemoteEndPoint:{serverConnectorContext.Socket.RemoteEndPoint.ToString()} module:{module}  command:{command} packet:{packet}");

            if (module == OnMessagePacket.MODULE && command == OnMessagePacket.COMMAND)
            {
                var msgPacket = new OnMessagePacket()
                {
                    Message = ((SendGroupMessagePacket)packet).Message,
                };

                _serverConnectors.Send(x => true, module, command, msgPacket);
            }


        }

        private static void ServerConnectors_OnNewConnector(ServerConnectorContext serverConnectorContext)
        {
            Console.WriteLine($"ServerConnectors_OnNewConnector RemoteEndPoint:{serverConnectorContext.Socket.RemoteEndPoint.ToString()}");
        }

        //static Dictionary<Tuple<int, int>, Type> GetTypeMap()
        //{
        //    return new Dictionary<Tuple<int, int>, Type>()
        //    {
        //       { new Tuple<int,int>(1,1) , typeof(string) },
        //       { new Tuple<int,int>(2,1) , typeof(string) },
        //       { new Tuple<int,int>(3,1) , typeof(LoginRequestPacket) }
        //    };
        //}
    }
}
