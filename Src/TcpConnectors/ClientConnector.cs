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
            byte[] payloadBuf = BinaryConverter.BinaryConvert.SerializeObject(packet.GetType(), packet.ToString());
            byte[] output = new byte[2 + payloadBuf.Length];

            output[0] = (byte)module;
            output[1] = (byte)command;
            Array.Copy(payloadBuf, 0, output, 2, payloadBuf.Length);

            TcpSocketsUtils.Send(_socket, output, OnSend, OnExcp);
        }

        public async void SendRequest(int command, object packet)
        {

        }

        public event Action<int, int, object> OnPacket;

        public event Action<int> OnDisconnect;

        public event Action<Exception> OnError;
    }
}
