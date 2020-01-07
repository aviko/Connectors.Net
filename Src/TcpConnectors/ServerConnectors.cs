using System;
using System.Collections.Generic;
using System.Threading;
using TcpConnectors.Utils;

namespace TcpConnectors
{


    public class ServerConnectorsSettings
    {
        public Dictionary<Tuple<int, int>, Type> PacketsMap { get; set; } = new Dictionary<Tuple<int, int>, Type>();
        public int ListenPort { get; set; } = 30;
        public int KeepAliveGraceInterval { get; set; } = 120;
        public int KeepAliveTimerInterval { get; set; } = 10;
        public int KeepAliveDisconnectInterval { get; set; } = 30;
        public int ReceiveBufferSize { get; set; } = 0;
    }


    public partial class ServerConnectors
    {
        public ServerConnectors(ServerConnectorsSettings settings)
        {

            _settings = settings;
            _settings.PacketsMap.Add(new Tuple<int, int>(0, 0), typeof(long)); // keep alive
        }

        public void Configure(Dictionary<Tuple<int, int>, Type> packetsMap)
        {

        }

        public void Listen()
        {
            new Thread(StartListeningBlocking).Start();
        }

        public void Send(Func<ServerConnectorContext, bool> filter, int module, int command, object packet)
        {
            byte[] output = ConnectorsUtils.SerializePacket(module, command, packet);

            foreach (var connector in _contextMap.Values)
            {
                if (filter(connector))
                {
                    TcpSocketsUtils.Send(connector.Socket, output, connector.OnSend, connector.OnExcp);
                }
            }
        }

        public void Send(int contextId, int module, int command, object packet)
        {
            if (_contextMap.TryGetValue(contextId, out var serverConnectorContext))
            {
                byte[] output = ConnectorsUtils.SerializePacket(module, command, packet);
                TcpSocketsUtils.Send(serverConnectorContext.Socket, output, serverConnectorContext.OnSend, serverConnectorContext.OnExcp);
            }
        }

        public event Action<ServerConnectorContext> OnNewConnector;

        public event Action<ServerConnectorContext, int, int, object> OnPacket;

        public event Func<ServerConnectorContext, int, int, object, object> OnRequestPacket;

        public event Action<ServerConnectorContext, int, int, int, object, Action<ServerConnectorContext, int, int, int, object, bool, int, int, Exception>> OnRequestMultiResponses;

        public event Action<ServerConnectorContext> OnDisconnect;

        public event Action<ServerConnectorContext, Exception> OnException;

        public event Action<ServerConnectorContext, DebugLogType, string> OnDebugLog;

    }
}
