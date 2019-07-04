using System;
using System.Collections.Generic;
using System.Text;
using TcpConnectors.TestCommon;

namespace TcpConnectors.TestServer
{
    partial class ConnectorsHandler
    {
        private readonly ServerConnectors _serverConnectors;


        public ConnectorsHandler()
        {
            _serverConnectors = new ServerConnectors(PacketsUtils.GetClient2ServerMapping());
            _serverConnectors.OnNewConnector += ServerConnectors_OnNewConnector;
            _serverConnectors.OnPacket += ServerConnectors_OnPacket; ;
            _serverConnectors.OnRequestPacket += ServerConnectors_OnRequestPacket;
            _serverConnectors.OnDisconnect += ServerConnectors_OnDisconnect;
            _serverConnectors.OnException += ServerConnectors_OnException;

            _serverConnectors.Listen(1111);

        }


        private void ServerConnectors_OnException(ServerConnectorContext connectorContext, Exception exp)
        {
            var remoteEndPoint = "NA";
            try { remoteEndPoint = connectorContext.Socket.RemoteEndPoint.ToString(); } catch { }

            Console.WriteLine($"ServerConnectors_OnException RemoteEndPoint:{remoteEndPoint} ex:{exp.ToString()}");
        }

        private void ServerConnectors_OnDisconnect(ServerConnectorContext serverConnectorContext)
        {
            var remoteEndPoint = "NA";
            try { remoteEndPoint = serverConnectorContext.Socket.RemoteEndPoint.ToString(); } catch { }

            Console.WriteLine($"ServerConnectors_OnDisconnect RemoteEndPoint:{remoteEndPoint}");
        }

        private object ServerConnectors_OnRequestPacket(ServerConnectorContext serverConnectorContext, int module, int command, object packet)
        {
            Console.WriteLine($"ServerConnectors_OnRequestPacket RemoteEndPoint:{serverConnectorContext.Socket.RemoteEndPoint.ToString()} module:{module}  command:{command} packet:{packet}");
            return HandleRequestPacket(serverConnectorContext, module, command, (dynamic)packet); 
        }

        private void ServerConnectors_OnPacket(ServerConnectorContext serverConnectorContext, int module, int command, object packet)
        {
            Console.WriteLine($"ServerConnectors_OnPacket RemoteEndPoint:{serverConnectorContext.Socket.RemoteEndPoint.ToString()} module:{module}  command:{command} packet:{packet}");

            HandlePacket(serverConnectorContext, module, command, (dynamic)packet);
        }

        private static void ServerConnectors_OnNewConnector(ServerConnectorContext serverConnectorContext)
        {
            Console.WriteLine($"ServerConnectors_OnNewConnector RemoteEndPoint:{serverConnectorContext.Socket.RemoteEndPoint.ToString()}");
        }


    }
}
