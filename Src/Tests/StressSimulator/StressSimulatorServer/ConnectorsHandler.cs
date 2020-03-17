using Newtonsoft.Json;
using StressSimulatorCommon;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using TcpConnectors;

namespace StressSimulatorServer
{
    class ConnectorsHandler
    {
        private ServerConnectors _serverConnectors;

        public ConnectorsHandler()
        {
            _serverConnectors = new ServerConnectors(new ServerConnectorsSettings()
            {
                PacketsMap = SendPacketsUtils.GetClient2ServerMapping(Assembly.GetAssembly(typeof(LoginRequestPacket))),
                ListenPort = 1112,
            });
            _serverConnectors.OnNewConnector += ServerConnectors_OnNewConnector;
            _serverConnectors.OnPacket += ServerConnectors_OnPacket; ;
            _serverConnectors.OnRequestPacket += ServerConnectors_OnRequestPacket;
            _serverConnectors.OnDisconnect += ServerConnectors_OnDisconnect;
            _serverConnectors.OnException += ServerConnectors_OnException;
            _serverConnectors.OnDebugLog += ServerConnectors_OnDebugLog;

            _serverConnectors.Listen();
        }

        public void SendToAll(SendPacketsUtils.IServer2ClientPacket server2ClientPacket)
        {
            _serverConnectors.Send(x => true, server2ClientPacket);
        }

        private object ServerConnectors_OnRequestPacket(ServerConnectorContext connectorContext, int module, int command, object packet)
        {
            var remoteEndPoint = "NA";
            try { remoteEndPoint = connectorContext.Socket.RemoteEndPoint.ToString(); } catch { }
            if (packet is LoginRequestPacket)
            {
                Console.WriteLine($"ServerConnectors_OnRequestPacket RemoteEndPoint:{remoteEndPoint} LoginRequestPacket:{JsonConvert.SerializeObject(packet)}");
                return new LoginResponsePacket() { RetCode = true, Message = "OK" };
            }
            if (packet is ReportRequestPacket)
            {
                Console.WriteLine($"ServerConnectors_OnRequestPacket RemoteEndPoint:{remoteEndPoint} ReportRequestPacket:{JsonConvert.SerializeObject(packet)}");
                var records = new List<string>(100);
                for (int i = 0; i < 100; i++) records.Add("this is record" + i);



                return new ReportResponsePacket() { Records = records };
            }
            return null;
        }

        private void ServerConnectors_OnDebugLog(ServerConnectorContext connectorContext, DebugLogType logType, string info)
        {
            var remoteEndPoint = "NA";
            try { remoteEndPoint = connectorContext.Socket.RemoteEndPoint.ToString(); } catch { }

            //Console.WriteLine($"ServerConnectors_OnDebugLog RemoteEndPoint:{remoteEndPoint} logType:{logType} info:{info}");
        }

        private void ServerConnectors_OnException(ServerConnectorContext connectorContext, Exception exp)
        {
            var remoteEndPoint = "NA";
            try { remoteEndPoint = connectorContext.Socket.RemoteEndPoint.ToString(); } catch { }

            // Console.WriteLine($"ServerConnectors_OnException RemoteEndPoint:{remoteEndPoint} ex:{exp.ToString()}");
        }

        private void ServerConnectors_OnDisconnect(ServerConnectorContext serverConnectorContext)
        {
            var remoteEndPoint = "NA";
            try { remoteEndPoint = serverConnectorContext.Socket.RemoteEndPoint.ToString(); } catch { }

            // Console.WriteLine($"ServerConnectors_OnDisconnect RemoteEndPoint:{remoteEndPoint}");
        }

        //private object ServerConnectors_OnRequestPacket(ServerConnectorContext serverConnectorContext, int module, int command, object packet)
        //{

        //}

        private void ServerConnectors_OnPacket(ServerConnectorContext serverConnectorContext, int module, int command, object packet)
        {
            Console.WriteLine($"ServerConnectors_OnPacket RemoteEndPoint:{serverConnectorContext.Socket.RemoteEndPoint.ToString()} \r\n module:{module}  command:{command} \r\n packet:{packet.GetType().Name + ": " + JsonConvert.SerializeObject(packet)}");

            //sends echo
            _serverConnectors.Send(serverConnectorContext.Id, 1, 1, "echo: " + packet.ToString());
        }

        private void ServerConnectors_OnNewConnector(ServerConnectorContext serverConnectorContext)
        {
            // Console.WriteLine($"ServerConnectors_OnNewConnector RemoteEndPoint:{serverConnectorContext.Socket.RemoteEndPoint.ToString()}");
        }

    }
}
