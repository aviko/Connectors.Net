using System;
using System.Threading.Tasks;

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

                new Task(() => HandleRequestResponse(rrData)).Start();


            }
            else //packet
            {
                var packet = ConnectorsUtils.DeserializePacket(buf, _serverConnectors._typeMap);
                _serverConnectors.TriggerOnPacket(this, buf[0], buf[1], packet);
            }

        }

        private void HandleRequestResponse(RequestResponseData rrData)
        {
            var res = _serverConnectors.TriggerOnRequestPacket(this, rrData.Module, rrData.Command, rrData.Packet);
            var resBuf = ConnectorsUtils.SerializeRequestPacket(rrData.Module, rrData.Command, res, rrData.RequestId);
            TcpSocketsUtils.Send(Socket, resBuf, OnSend, OnExcp);
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
