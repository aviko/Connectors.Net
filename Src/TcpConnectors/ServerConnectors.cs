using System;
using System.Collections.Generic;
using System.Threading;

namespace TcpConnectors
{
    public partial class ServerConnectors
    {
        public ServerConnectors(Dictionary<Tuple<int, int>, Type> typeMap)
        {
            _typeMap = typeMap;
            _typeMap.Add(new Tuple<int, int>(0, 0), typeof(long)); // keep alive
        }

        public void Configure(Dictionary<Tuple<int, int>, Type> typeMap)
        {

        }

        public void Listen(int port)
        {
            _port = port;
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

        public event Action<ServerConnectorContext> OnDisconnect;

        public event Action<ServerConnectorContext, Exception> OnException;
    }
}
