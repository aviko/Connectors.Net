using System;
using System.Collections.Concurrent;
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
        public ConcurrentDictionary<string, object> Map { get; set; } = new ConcurrentDictionary<string, object>();

        public long TotalDataBytesReceived { get; private set; } = 0;
        public long TotalGrossBytesReceived { get; private set; } = 0;
        public long TotalDataBytesSent { get; private set; } = 0;
        public long TotalGrossBytesSent { get; private set; } = 0;
    }
}
