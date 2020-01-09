using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using TcpConnectors;
using TestRequestMultiResponseCommon;

namespace TestRequestMultiResponseServer
{
    class Program
    {
        private static ServerConnectors _serverConnectors;

        static void Main(string[] args)
        {
            Console.WriteLine("TestSimpleEchoServer");

            _serverConnectors = new ServerConnectors(new ServerConnectorsSettings()
            {
                PacketsMap = new Dictionary<Tuple<int, int>, Type>() {
                    { new Tuple<int, int>(1, 1), typeof(GetListRequestPacket) },
                    { new Tuple<int, int>(1, 2), typeof(GetListRequestPacket) },
                },
                ListenPort = 1112,
            });
            _serverConnectors.OnNewConnector += ServerConnectors_OnNewConnector;
            _serverConnectors.OnPacket += ServerConnectors_OnPacket; ;
            _serverConnectors.OnRequestPacket += ServerConnectors_OnRequestPacket;
            _serverConnectors.OnDisconnect += ServerConnectors_OnDisconnect;
            _serverConnectors.OnException += ServerConnectors_OnException;
            _serverConnectors.OnDebugLog += ServerConnectors_OnDebugLog;

            _serverConnectors.OnRequestMultiResponses += ServerConnectors_OnRequestMultiResponses;

            _serverConnectors.Listen();
        }

        private static void ServerConnectors_OnRequestMultiResponses(
            ServerConnectorContext serverConnectorContext, int module, int command, int requestId, object packet,
            Action<ServerConnectorContext, int, int, int, object, bool, int, int, Exception> callback)
        {
            Console.WriteLine("ServerConnectors_OnRequestMultiResponses");

            var list = new List<string>();

            for (int i = 0; i < 35_000; i++)
            {
                list.Add(i.ToString());
                if (i % 1000000 == 0) Console.WriteLine(i);
            }

            callback(
                serverConnectorContext, module, command, requestId,
                new GetListResponsePacket() { List = list },
                true, 1, 1, null);
        }

        private static object ServerConnectors_OnRequestPacket(ServerConnectorContext connectorContext, int module, int command, object packet)
        {
            var list = new List<string>();

            for (int i = 0; i < 35_000_000; i++)
            {
                list.Add(i.ToString());
                if (i % 1000000 == 0) Console.WriteLine(i);
            }

            return new GetListResponsePacket() { List = list };
        }

        private static void ServerConnectors_OnDebugLog(ServerConnectorContext connectorContext, DebugLogType logType, string info)
        {
            var remoteEndPoint = "NA";
            try { remoteEndPoint = connectorContext.Socket.RemoteEndPoint.ToString(); } catch { }

            Console.WriteLine($"ServerConnectors_OnDebugLog RemoteEndPoint:{remoteEndPoint} logType:{logType} info:{info}");
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

        //private object ServerConnectors_OnRequestPacket(ServerConnectorContext serverConnectorContext, int module, int command, object packet)
        //{

        //}

        private static void ServerConnectors_OnPacket(ServerConnectorContext serverConnectorContext, int module, int command, object packet)
        {
            Console.WriteLine($"ServerConnectors_OnPacket RemoteEndPoint:{serverConnectorContext.Socket.RemoteEndPoint.ToString()} \r\n module:{module}  command:{command} \r\n packet:{packet.GetType().Name + ": " + JsonConvert.SerializeObject(packet)}");

            //sends echo
            _serverConnectors.Send(serverConnectorContext.Id, 1, 1, "echo: " + packet.ToString());
        }

        private static void ServerConnectors_OnNewConnector(ServerConnectorContext serverConnectorContext)
        {
            Console.WriteLine($"ServerConnectors_OnNewConnector RemoteEndPoint:{serverConnectorContext.Socket.RemoteEndPoint.ToString()}");
        }
    }
}
