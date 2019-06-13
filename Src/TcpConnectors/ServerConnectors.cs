using System;
using System.Collections.Generic;

namespace TcpConnectors
{
    public partial class ServerConnectors
    {
        public ServerConnectors()
        {

        }

        public void Configure(Dictionary<Tuple<int, int>, Type> typeMap)
        {

        }

        public void Listen(int port)
        {

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
