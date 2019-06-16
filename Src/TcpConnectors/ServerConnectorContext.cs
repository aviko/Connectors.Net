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


        public Socket Socket { get; internal set; }
        public object Data { get; set; }
        public Dictionary<string, object> Map { get; set; } = new Dictionary<string, object>();

        internal void OnRecv(byte[] buf)
        {
            Console.WriteLine("ServerConnectorContext.OnRecv()");

            var destBuf = new byte[buf.Length - 2];
            Array.Copy(buf, 2, destBuf, 0, buf.Length - 2);

            var packet = BinaryConverter.BinaryConvert.DeserializeObject<string>(destBuf);

            Console.WriteLine($"module:{buf[0]}  command:{buf[1]} packet:{packet} ");

        }

        internal void OnExcp(Exception e)
        {
            Console.WriteLine("ServerConnectorContext.OnExcp()");
        }
    }
}
