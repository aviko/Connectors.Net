using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace TcpConnectors
{
    partial class ServerConnectorContext
    {
        internal class RequestResponseData
        {
            internal int RequestId { get; set; }
            internal int Module { get; set; }
            internal int Command { get; set; }
            internal object Packet { get; set; }
        }


        private ServerConnectors _serverConnectors;
        private BlockingCollection<RequestResponseData> _requestResponseQueue = new BlockingCollection<RequestResponseData>();

        internal ServerConnectorContext(int id, ServerConnectors serverConnectors)
        {
            Id = id;
            _serverConnectors = serverConnectors;
        }

        internal void OnRecv(byte[] buf)
        {
            if (buf[0] == 0) //request response packet
            {
                var reqPacket = ConnectorsUtils.DeserializeRequestPacket(buf, _serverConnectors._typeMap, out var requestId);
                var rrData = new RequestResponseData()
                {
                    RequestId = requestId,
                    Module = buf[5],
                    Command =  buf[6],
                    Packet = reqPacket,
                };

                var res = _serverConnectors.TriggerOnRequestPacket(this, buf[5], buf[6], reqPacket);
                var resBuf = ConnectorsUtils.SerializeRequestPacket(buf[5], buf[6], res, requestId);
                TcpSocketsUtils.Send(Socket, resBuf, OnSend, OnExcp);
            }
            else //packet
            {
                var packet = ConnectorsUtils.DeserializePacket(buf, _serverConnectors._typeMap);
                _serverConnectors.TriggerOnPacket(this, buf[0], buf[1], packet);
            }

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
