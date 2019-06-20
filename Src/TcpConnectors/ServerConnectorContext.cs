using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace TcpConnectors
{
    public partial class ServerConnectorContext
    {
        public int Id { get; private set; }
        public Socket Socket { get; internal set; }
        public object Data { get; set; }
        public Dictionary<string, object> Map { get; set; } = new Dictionary<string, object>();
    }
}
