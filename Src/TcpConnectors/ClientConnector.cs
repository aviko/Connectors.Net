using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TcpConnectors.Utils;

namespace TcpConnectors
{
    //todos:
    // verify that user never use module 0

    public class ClientConnectorSettings
    {
        public Dictionary<Tuple<int, int>, Type> PacketsMap { get; set; } = new Dictionary<Tuple<int, int>, Type>();
        public bool AutoReconnect { get; set; } = true;
        public int ReconnectInterval { get; set; } = 10;
        public int KeepAliveDisconnectInterval { get; set; } = 30;
        public List<Tuple<string, int>> ServerAddressList { get; set; } = new List<Tuple<string, int>>();
        public int ReceiveBufferSize { get; set; } = 0;
    }

    //todo: dispose
    public partial class ClientConnector : IDisposable
    {
        public bool IsConnected { get; private set; } = false;

        public ClientConnector(ClientConnectorSettings settings)
        {
            Init(settings);
        }

        //wait up to [3] seconds to connect, 
        //return true if connected
        public bool Connect()
        {
            new Task(ConnectInternal).Start();
            return _connectEvent.WaitOne(3000); //todo: from settings
        }

        public void Send(int module, int command, object packet)
        {
            if (IsConnected == false)
            {
                throw new InvalidOperationException("Connector not connected");
            }

            byte[] output = ConnectorsUtils.SerializePacket(module, command, packet);
            TcpSocketsUtils.Send(_socket, output, OnSend, OnExcp);
        }

        public void SendRequestMultiResponses(int module, int command, object packet, RequestMultiResponsesClientCallback responseCallback)
        {
            if (IsConnected == false)
            {
                throw new InvalidOperationException("Connector not connected");
            }
            var requestId = _nextRequestId += 2;
            byte[] output = ConnectorsUtils.SerializeRequestPacket(ConnectorsUtils.RequestTypeRequestMultiResponses, module, command, packet, requestId);

            _reqMultiResHandler.Request(
                requestId, () => { TcpSocketsUtils.Send(_socket, output, OnSend, OnExcp); },
                responseCallback);//,                responseType);
        }


        public object SendRequest(int module, int command, object packet)
        {
            if (IsConnected == false)
            {
                throw new InvalidOperationException("Connector not connected");
            }
            var requestId = _nextRequestId += 2;
            byte[] output = ConnectorsUtils.SerializeRequestPacket(ConnectorsUtils.RequestTypeRequestResponse, module, command, packet, requestId);
            var res = _reqResHandler.Request(requestId, () => { TcpSocketsUtils.Send(_socket, output, OnSend, OnExcp); });
            return res;
        }

        public async Task<object> SendRequestAsync(int module, int command, object packet)
        {
            if (IsConnected == false)
            {
                throw new InvalidOperationException("Connector not connected");
            }
            var requestId = _nextRequestIdAsync += 2;
            byte[] output = ConnectorsUtils.SerializeRequestPacket(ConnectorsUtils.RequestTypeRequestResponse, module, command, packet, requestId);
            var res = await _reqResAsyncHandler.Request(requestId, () => { TcpSocketsUtils.Send(_socket, output, OnSend, OnExcp); });
            return res;
        }

        public event Action<int, int, object> OnPacket;

        public event Action OnConnect;

        public event Action OnDisconnect;

        public event Action<Exception> OnException;

        public event Action<DebugLogType, string> OnDebugLog;
    }

    public enum DebugLogType
    {
        ConnectFailed = 1,
        OnSend,
        OnRecvException,
        OnRequestResponseException,
        OnKeepAlive,
        Info,
    }
}
