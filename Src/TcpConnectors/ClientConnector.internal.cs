using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace TcpConnectors
{
    partial class ClientConnector
    {
        private Socket _socket;
        private Dictionary<Tuple<int, int>, Type> _typeMap;
        private int _nextRequestId = 1;

        private BlockingRequestResponseHandler<int, object> _reqResHandler = new BlockingRequestResponseHandler<int, object>();


        private void OnRecv(byte[] buf)
        {
            if (buf[0] == 0) //request response packet
            {
                var reqPacket = ConnectorsUtils.DeserializeRequestPacket(buf, _typeMap, out var requestId);
                _reqResHandler.HandleResponse(requestId, reqPacket);
            }
            else //packet
            {
                object packet = ConnectorsUtils.DeserializePacket(buf, _typeMap);
                OnPacket?.Invoke(buf[0], buf[1], packet);
            }

        }

        private void OnExcp(Exception e)
        {
            OnDisconnect?.Invoke();
        }

        private void OnSend()
        {
            Console.WriteLine("ClientConnector.OnSend");
        }
    }
}
