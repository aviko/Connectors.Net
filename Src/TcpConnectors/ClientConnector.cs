using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace TcpConnectors
{
    public partial class ClientConnector
    {
        public ClientConnector(Dictionary<Tuple<int, int>, Type> typeMap)
        {
            _typeMap = typeMap;
        }


        public void Connect(string host, int port)
        {
            _socket = TcpSocketsUtils.Connect(host, port);
            TcpSocketsUtils.Recv(_socket, OnRecv, OnExcp, TcpSocketsUtils.ms_DefualtReceiveBufferSize, true);


        }

        public void Send(int module, int command, object packet)
        {
            byte[] output = ConnectorsUtils.SerializePacket(module, command, packet);

            TcpSocketsUtils.Send(_socket, output, OnSend, OnExcp);
        }


        public object SendRequest(int module, int command, object packet)
        {
            var requestId = _nextRequestId += 2;
            byte[] output = ConnectorsUtils.SerializeRequestPacket(module, command, packet, requestId);
            var res = _reqResHandler.Request(requestId, () => { TcpSocketsUtils.Send(_socket, output, OnSend, OnExcp); });
            return res;
        }

        public async Task<object> SendRequestAsync(int module, int command, object packet)
        {
            var requestId = _nextRequestIdAsync += 2;
            byte[] output = ConnectorsUtils.SerializeRequestPacket(module, command, packet, requestId);
            var res = await _reqResAsyncHandler.Request(requestId, () => { TcpSocketsUtils.Send(_socket, output, OnSend, OnExcp); });
            return res;
        }

        public event Action<int, int, object> OnPacket;

        public event Action OnDisconnect;

        public event Action<Exception> OnException;
    }
}
