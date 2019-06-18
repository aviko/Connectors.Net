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



        private void OnRecv(byte[] buf)
        {
            object packet = ConnectorsUtils.DeserializePacket(buf, _typeMap);

            OnPacket?.Invoke(buf[0], buf[1], packet);
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
