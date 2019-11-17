using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Timers;

namespace TcpConnectors
{
    partial class ServerConnectors
    {

        private bool _isDisposed = false;
        private Socket _listenerSock;
        private int _nextContextId = 1;
        internal ConcurrentDictionary<int, ServerConnectorContext> _contextMap = new ConcurrentDictionary<int, ServerConnectorContext>();
        internal ServerConnectorsSettings _settings;

        private BlockingCollection<Tuple<ServerConnectorContext, int, int, object>> _packetsQueue = new BlockingCollection<Tuple<ServerConnectorContext, int, int, object>>();

        private long _keepAliveTimestamp = 0;
        private System.Timers.Timer _keepAliveTimer = null;


        private void StartListeningBlocking()
        {

            IPEndPoint ep = new IPEndPoint(IPAddress.IPv6Any, _settings.ListenPort);

            _listenerSock = new Socket(
                IPAddress.IPv6Any.AddressFamily,
                SocketType.Stream,
                ProtocolType.Tcp);

            _listenerSock.SetSocketOption(SocketOptionLevel.IPv6, (SocketOptionName)27, false);

            _listenerSock.Bind(ep);
            _listenerSock.Listen(10);

            if (_keepAliveTimer == null)
            {
                _keepAliveTimer = new System.Timers.Timer(_settings.KeepAliveTimerInterval * 1000);
                _keepAliveTimer.Elapsed += KeepAliveTimer_Elapsed; ;
                _keepAliveTimer.Start();
            }

            new Thread(PacketsQueueWorker).Start();


            while (!_isDisposed)
            {
                try
                {
                    // Start an asynchronous socket to listen for connections.
                    Socket newSocket = _listenerSock.Accept();
                    int id = _nextContextId++;
                    var newContext = new ServerConnectorContext(id, this);
                    newContext.Socket = newSocket;
                    _contextMap[id] = newContext;
                    OnNewConnector?.Invoke(newContext);
                    TcpSocketsUtils.Recv(
                        newSocket,
                        newContext.OnRecv,
                        newContext.OnExcp,
                        _settings.ReceiveBufferSize == 0 ? TcpSocketsUtils.ms_DefualtReceiveBufferSize : _settings.ReceiveBufferSize,
                        true);
                }
                catch (Exception ex)
                {
                    if (!_isDisposed)
                    {
                        OnException?.Invoke(null, ex);
                    }
                }
            }
        }

        private void KeepAliveTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _keepAliveTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            foreach (var connector in _contextMap.Values)
            {
                bool needToDisconnect = true;
                //check new context
                if ((DateTime.UtcNow - connector._connectedTime).TotalSeconds < _settings.KeepAliveGraceInterval) needToDisconnect = false;

                //_lastRecievedLeepAliveTimestamp less than 30 seconds
                if ((_keepAliveTimestamp - connector._lastRecievedLeepAliveTimestamp) < _settings.KeepAliveDisconnectInterval) needToDisconnect = false;

                if (needToDisconnect)
                {
                    Console.WriteLine("needToDisconnect");
                    try { connector.Socket.Close(); } catch { }
                    connector.OnExcp(null);
                    continue;
                }

                //send new keep alive
                var keepaliveBuf = ConnectorsUtils.SerializeRequestPacket(ConnectorsUtils.RequestTypeKeepAlive, 0, 0, _keepAliveTimestamp, 0);
                TcpSocketsUtils.Send(connector.Socket, keepaliveBuf, connector.OnSend, connector.OnExcp);
            }
        }

        internal void TriggerOnPacket(ServerConnectorContext serverConnectorContext, int module, int command, object packet)
        {
            _packetsQueue.Add(new Tuple<ServerConnectorContext, int, int, object>(serverConnectorContext, module, command, packet));
        }

        internal object TriggerOnRequestPacket(ServerConnectorContext serverConnectorContext, int module, int command, object packet)
        {
            return OnRequestPacket?.Invoke(serverConnectorContext, module, command, packet);
        }

        internal void TriggerOnDisconnect(ServerConnectorContext serverConnectorContext)
        {
            OnDisconnect?.Invoke(serverConnectorContext);
        }

        internal void TriggerOnException(ServerConnectorContext serverConnectorContext, Exception ex)
        {
            OnException?.Invoke(serverConnectorContext, ex);
        }

        internal void TriggerOnDebugLog(ServerConnectorContext serverConnectorContext, DebugLogType logType, string info)
        {
            OnDebugLog?.Invoke(serverConnectorContext, logType, info);
        }


        private void PacketsQueueWorker()
        {
            while (!_isDisposed) 
            {
                var tuple = _packetsQueue.Take();
                OnPacket?.Invoke(tuple.Item1, tuple.Item2, tuple.Item3, tuple.Item4);//context, module, command, packet

            }
        }


    }
}
