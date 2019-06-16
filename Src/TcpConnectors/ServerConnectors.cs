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

        }

        public void Send(ServerConnectorContext context, int module, int command, object packet)
        {

        }

        public event Action<ServerConnectorContext> OnNewConnector;

        public event Action<ServerConnectorContext, int, int, object> OnPacket;

        public event Action<ServerConnectorContext, int, int, object> OnRequestPacket;

        public event Action<ServerConnectorContext, int> OnDisconnect;

        public event Action<ServerConnectorContext, Exception> OnError;
    }
}
