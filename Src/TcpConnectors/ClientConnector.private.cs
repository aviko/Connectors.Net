using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace TcpConnectors
{
    partial class ClientConnector
    {
        protected Socket _socket;




        private void OnRecv(byte[] buf)
        {
            Console.WriteLine("ClientConnector.OnRecv");
        }

        private void OnExcp(Exception e)
        {
            Console.WriteLine("ClientConnector.OnExcp");
        }

        private void OnSend()
        {
            Console.WriteLine("ClientConnector.OnSend");
        }
    }
}
