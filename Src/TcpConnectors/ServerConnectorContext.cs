using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace TcpConnectors
{
    public class ServerConnectorContext
    {
        private ServerConnectors _serverConnectors;

        internal ServerConnectorContext(int id, ServerConnectors serverConnectors)
        {
            Id = id;
            _serverConnectors = serverConnectors;
        }


        public int Id { get; private set; }
        public Socket Socket { get; internal set; }
        public object Data { get; set; }
        public Dictionary<string, object> Map { get; set; } = new Dictionary<string, object>();

        internal void OnRecv(byte[] buf)
        {

            var packet = ConnectorsUtils.DeserializePacket(buf, _serverConnectors._typeMap);
            _serverConnectors.TriggerOnPacket(this, buf[0], buf[1], packet);

        }

        internal void OnSend()
        {
            Console.WriteLine("ServerConnectorContext.OnSend()");
        }

        internal void OnExcp(Exception ex)
        {
            if (Socket.Connected)
            {
                _serverConnectors.TriggerOnException(this, ex);
            }
            else
            {
                //not connected
                if (_serverConnectors._contextMap.TryRemove(this.Id, out var removed))
                {
                    _serverConnectors.TriggerOnDisconnect(this);
                }
            }

        }
    }
}
