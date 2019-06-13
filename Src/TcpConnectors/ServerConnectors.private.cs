using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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

        private void StartListeningBlocking(int port)
        {
            while (!_isDisposed)
            {
                try
                {
                    // Start an asynchronous socket to listen for connections.
                    Socket newSocket = _listenerSock.Accept();
                    var newContext = new ServerConnectorContext(this);
                    _contextMap[_nextContextId++] = newContext;
                    TcpSocketsUtils.Recv(newSocket, newContext.OnRecv, newContext.OnExcp, TcpSocketsUtils.ms_DefualtReceiveBufferSize, false);
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
