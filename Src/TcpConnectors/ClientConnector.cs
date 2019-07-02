using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace TcpConnectors
{
    public class ClientConnectorSettings
    {
        public Dictionary<Tuple<int, int>, Type> TypesMap { get; set; } = new Dictionary<Tuple<int, int>, Type>();
        public bool AutoReconnect { get; set; } = true;
        public int KeepAliveInterval { get; set; } = 30;
        public List<Tuple<string, int>> ServerAddressList { get; set; } = new List<Tuple<string, int>>();
    }


    public partial class ClientConnector
    {
        public bool IsConnected { get; private set; } = false;

        public ClientConnector(ClientConnectorSettings settings)
        {
            _settings = settings;
            _settings.TypesMap.Add(new Tuple<int, int>(0, 0), typeof(long)); // keep alive

        }

        public void Connect()
        {
            ConnectInternal();
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

        public event Action OnConnect;

        public event Action OnDisconnect;

        public event Action<Exception> OnException;
    }
}
