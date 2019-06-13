using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace TcpConnectors
{
    public class ServerConnectorContext
    {
        private ServerConnectors _serverConnectors;

        internal ServerConnectorContext(ServerConnectors serverConnectors)
        {
            _serverConnectors = serverConnectors;
        }


        public Socket Socket { get; set; }
        public object Data { get; set; }
        public Dictionary<string, object> Map { get; set; } = new Dictionary<string, object>();

        internal bool OnRecv(byte[] buf)
        {
            return true;
        }

        internal void OnExcp(Exception e)
        {

        }
    }
}
