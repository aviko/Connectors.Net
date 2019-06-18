using System;
using System.Collections.Generic;
using System.Text;

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


        public async void SendRequest(int command, object packet)
        {

        }

        public event Action<int, int, object> OnPacket;

        public event Action OnDisconnect;

        public event Action<Exception> OnException;
    }
}
