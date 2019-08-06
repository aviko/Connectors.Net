using Newtonsoft.Json;
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
            _serverConnectors = new ServerConnectors(new ServerConnectorsSettings()
            {
                PacketsMap = PacketsUtils.GetClient2ServerMapping(),
                ListenPort = 1111,
            });
            _serverConnectors.OnNewConnector += ServerConnectors_OnNewConnector;
            _serverConnectors.OnPacket += ServerConnectors_OnPacket; ;
            _serverConnectors.OnRequestPacket += ServerConnectors_OnRequestPacket;
            _serverConnectors.OnDisconnect += ServerConnectors_OnDisconnect;
            _serverConnectors.OnException += ServerConnectors_OnException;
            _serverConnectors.OnDebugLog += ServerConnectors_OnDebugLog; ;

            _serverConnectors.Listen();

        }

        private void ServerConnectors_OnDebugLog(ServerConnectorContext connectorContext, DebugLogType logType, string info)
        {
            var remoteEndPoint = "NA";
            try { remoteEndPoint = connectorContext.Socket.RemoteEndPoint.ToString(); } catch { }

            Console.WriteLine($"ServerConnectors_OnDebugLog RemoteEndPoint:{remoteEndPoint} logType:{logType} info:{info}");
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

            if (serverConnectorContext.Data != null)
            {
                Program.ChatServerModel.Logout(serverConnectorContext.Data.ToString(), serverConnectorContext);
            }
        }

        private object ServerConnectors_OnRequestPacket(ServerConnectorContext serverConnectorContext, int module, int command, object packet)
        {
            Console.WriteLine($"ServerConnectors_OnRequestPacket RemoteEndPoint:{serverConnectorContext.Socket.RemoteEndPoint.ToString()} \r\n module:{module}  command:{command} \r\n packet:{packet.GetType().Name + ": " + JsonConvert.SerializeObject(packet)}");
            var resPacket = HandleRequestPacket(serverConnectorContext, module, command, (dynamic)packet);
            Console.WriteLine($"===> res packet:{resPacket.GetType().Name + ": " + JsonConvert.SerializeObject(resPacket)}");
            return resPacket;
        }

        private void ServerConnectors_OnPacket(ServerConnectorContext serverConnectorContext, int module, int command, object packet)
        {
            Console.WriteLine($"ServerConnectors_OnPacket RemoteEndPoint:{serverConnectorContext.Socket.RemoteEndPoint.ToString()} \r\n module:{module}  command:{command} \r\n packet:{packet.GetType().Name + ": " + JsonConvert.SerializeObject(packet)}");

            HandlePacket(serverConnectorContext, module, command, (dynamic)packet);
        }

        private static void ServerConnectors_OnNewConnector(ServerConnectorContext serverConnectorContext)
        {
            Console.WriteLine($"ServerConnectors_OnNewConnector RemoteEndPoint:{serverConnectorContext.Socket.RemoteEndPoint.ToString()}");
        }


    }
}
