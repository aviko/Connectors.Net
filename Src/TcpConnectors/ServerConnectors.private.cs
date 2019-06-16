using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace TcpConnectors
{
    partial class ServerConnectors
    {

        private int _port;
        private bool _isDisposed = false;
        private Socket _listenerSock;
        private int _nextContextId = 1;
        private ConcurrentDictionary<int, ServerConnectorContext> _contextMap = new ConcurrentDictionary<int, ServerConnectorContext>();

        private void StartListeningBlocking()
        {

            IPEndPoint ep = new IPEndPoint(IPAddress.IPv6Any, _port);

            _listenerSock = new Socket(
                IPAddress.IPv6Any.AddressFamily,
                SocketType.Stream,
                ProtocolType.Tcp);

            _listenerSock.SetSocketOption(SocketOptionLevel.IPv6, (SocketOptionName)27, false);

            _listenerSock.Bind(ep);
            _listenerSock.Listen(10);

            while (!_isDisposed)
            {

                try
                {
                    // Start an asynchronous socket to listen for connections.
                    Socket newSocket = _listenerSock.Accept();
                    var newContext = new ServerConnectorContext(this);
                    newContext.Socket = newSocket;
                    _contextMap[_nextContextId++] = newContext;
                    OnNewConnector?.Invoke(newContext);
                    TcpSocketsUtils.Recv(newSocket, newContext.OnRecv, newContext.OnExcp, TcpSocketsUtils.ms_DefualtReceiveBufferSize, true);
                }
                catch (Exception e)
                {
                    if (!_isDisposed)
                    {

                    }
                }
            }
        }

    }
}
