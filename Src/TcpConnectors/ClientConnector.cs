using System;
using System.Collections.Generic;
using System.Text;

namespace TcpConnectors
{
    public class ClientConnector
    {
        public ClientConnector()
        {

        }

        public void Configure(Dictionary<Tuple<int, int>, Type> typeMap)
        {

        }

        public void Connect(string host, int port)
        {

        }

        public void Send(int module, int command, object packet)
        {

        }

        public async void SendRequest(int command, object packet)
        {
            
        }

        public event Action<int, int, object> OnPacket;

        public event Action<int> OnDisconnect;

        public event Action<Exception> OnError;
    }
}
