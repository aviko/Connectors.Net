using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using TcpConnectors.Utils;

namespace TcpConnectors
{
    partial class ClientConnector
    {
        private Socket _socket = null;
        private ClientConnectorSettings _settings;
        private int _nextRequestId = 1; //odd numbers
        private int _nextRequestIdAsync = 2; //even numbers
        private DateTime _lastKeepAliveTime = DateTime.UtcNow;
        private DateTime _lastRecvProgressTime = DateTime.UtcNow;
        private DateTime _lastRecvTime = DateTime.UtcNow;
        private System.Timers.Timer _reconnectTimer = null;
        private bool _isDisposed = false;
        private ManualResetEvent _connectEvent = new ManualResetEvent(false);

        private BlockingRequestResponseHandler<int, object> _reqResHandler = new BlockingRequestResponseHandler<int, object>();
        private AsyncRequestResponseHandler<int, object> _reqResAsyncHandler = new AsyncRequestResponseHandler<int, object>();
        private RequestMultiResponsesHandler<int> _reqMultiResHandler = new RequestMultiResponsesHandler<int>();

        private BlockingCollection<Tuple<int, int, object>> _packetsQueue = new BlockingCollection<Tuple<int, int, object>>();

        private void Init(ClientConnectorSettings settings)
        {
            _settings = settings;
            _settings.PacketsMap.Add(new Tuple<int, int>(0, 0), typeof(long)); // keep alive
            _settings.PacketsMap.Add(new Tuple<int, int>(0, 1), typeof(string)); // request response error

            new Thread(PacketsQueueWorker).Start();
        }

        private void OnRecv(byte[] buf)
        {
            try
            {
                OnDebugLog?.Invoke(DebugLogType.Info, "OnRecv - start");

                _lastRecvTime = DateTime.UtcNow;

                // module, command
                if (buf[0] == 0) //request response packet
                {
                    object reqPacket = null;
                    int requestId = 0;
                    byte module = 0, command = 0, requestType = 0;
                    try
                    {
                        requestType = buf[1];
                        OnDebugLog?.Invoke(DebugLogType.Info, $"OnRecv - requestType = {requestType}");
                        if (requestType != ConnectorsUtils.RequestTypeRequestMultiResponses)
                        {
                            reqPacket = ConnectorsUtils.DeserializeRequestPacket(buf, _settings.PacketsMap, out requestId, out module, out command);
                        }
                    }
                    catch (Exception ex) { OnDebugLog?.Invoke(DebugLogType.OnRecvException, ex.ToString()); }

                    if (requestType == ConnectorsUtils.RequestTypeKeepAlive) //keep alive
                    {
                        _lastKeepAliveTime = DateTime.UtcNow;
                        TcpSocketsUtils.Send(_socket, buf, OnSend, OnExcp);
                        OnDebugLog?.Invoke(DebugLogType.OnKeepAlive, reqPacket.ToString());
                        return;
                    }

                    if (requestType == ConnectorsUtils.RequestTypeRequestResponse)
                    {
                        if (requestId % 2 == 1)
                        {
                            if (module == 0 && command == 1)
                            {
                                _reqResHandler.HandleExceptionResponse(
                                    requestId,
                                    new Exception("Exception in Server, check inner exception", new Exception((reqPacket ?? "").ToString())));
                            }
                            else
                            {
                                _reqResHandler.HandleResponse(requestId, reqPacket);
                            }
                        }
                        else
                        {
                            if (module == 0 && command == 1)
                            {
                                _reqResAsyncHandler.HandleExceptionResponse(
                                    requestId,
                                    new Exception("Exception in Server, check inner exception", new Exception((reqPacket ?? "").ToString())));
                            }
                            else
                            {
                                _reqResAsyncHandler.HandleResponse(requestId, reqPacket);
                            }
                        }
                    }

                    if (requestType == ConnectorsUtils.RequestTypeRequestMultiResponses)
                    {
                        reqPacket = ConnectorsUtils.DeserializeMultiResponsePacket(
                                buf, _settings.PacketsMap, out requestId,
                                out bool isLast, out int nRecieved, out int nTotal,
                                out module, out command);

                        if (module == 0 && command == 1)
                        {
                            _reqMultiResHandler.HandleExceptionResponse(requestId, new Exception((reqPacket ?? "").ToString()));
                        }
                        else
                        {
                            OnDebugLog?.Invoke(DebugLogType.Info, $"OnRecv - ConnectorsUtils.RequestTypeRequestMultiResponses");

                            _reqMultiResHandler.HandleResponse(requestId, reqPacket, isLast, nRecieved, nTotal);
                        }
                    }

                }
                else //packet
                {
                    object packet = ConnectorsUtils.DeserializePacket(buf, _settings.PacketsMap, out byte module, out byte command);
                    _packetsQueue.Add(new Tuple<int, int, object>(module, command, packet));
                }
            }
            catch (Exception ex)
            {
                OnDebugLog?.Invoke(DebugLogType.OnRecvException, ex.ToString());
            }
        }

        internal void OnRecvProgress(int bytesRecived, int totalPacketLen)
        {
            Console.WriteLine($"OnRecvProgress bytesRecived:{bytesRecived}, totalPacketLen:{totalPacketLen}");
            _lastRecvProgressTime = DateTime.UtcNow;
        }

        private void OnExcp(Exception ex)
        {
            if (_socket != null && _socket.Connected == false)
            {
                DisconnectInternal();
            }
            else
            {
                OnException?.Invoke(ex);
            }
        }

        private void DisconnectInternal()
        {
            _reqResHandler.HandleExceptionResponseForAll(new Exception("Disconnect"));

            if (IsConnected)
            {
                IsConnected = false;
                OnDisconnect?.Invoke();
            }
        }

        private void OnSend()
        {
            OnDebugLog?.Invoke(DebugLogType.OnSend, "sent");

        }

        private bool _isInConnectInternal = false;
        private void ConnectInternal()
        {
            try
            {
                //start keepAlive timer only on first connect
                if (_reconnectTimer == null)
                {
                    _reconnectTimer = new System.Timers.Timer(_settings.ReconnectInterval * 1000);
                    _reconnectTimer.Elapsed += ReconnectTimer_Elapsed; ;
                    _reconnectTimer.Start();
                }

                if (_isInConnectInternal) return;
                _isInConnectInternal = true;

                if (_socket != null)
                {
                    try { _socket.Dispose(); } catch { }
                    _socket = null;
                }

                foreach (var serverAddress in _settings.ServerAddressList)
                {
                    try
                    {
                        _socket = TcpSocketsUtils.Connect(serverAddress.Item1, serverAddress.Item2);
                        break;
                    }
                    catch
                    {
                        OnDebugLog?.Invoke(DebugLogType.ConnectFailed, $" host:{serverAddress.Item1} port:{serverAddress.Item2} ");
                    }
                }
                _connectEvent.Set();


                if (_socket != null)
                {
                    IsConnected = true;
                    OnConnect?.Invoke();
                    TcpSocketsUtils.Recv(
                        _socket,
                        OnRecv,
                        OnExcp,
                        OnRecvProgress,
                        _settings.ReceiveBufferSize == 0 ? TcpSocketsUtils.ms_DefualtReceiveBufferSize : _settings.ReceiveBufferSize,
                        true);
                }
            }
            catch (Exception ex)
            {

            }
            finally
            {
                _isInConnectInternal = false;
            }

        }

        private void PacketsQueueWorker()
        {
            while (_isDisposed == false)
            {
                var tuple = _packetsQueue.Take();
                OnPacket?.Invoke(tuple.Item1, tuple.Item2, tuple.Item3);//module, command, packet

            }
        }

        private void ReconnectTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (_isDisposed) return;

            var isRecvActive = (DateTime.UtcNow - _lastRecvProgressTime).TotalSeconds < 10 || (DateTime.UtcNow - _lastRecvTime).TotalSeconds < 10;
            if (isRecvActive)
            {
                var recvInProgressBuf = ConnectorsUtils.SerializeRequestPacket(ConnectorsUtils.RequestTypeRecvInProgress, 0, 0, 0, 0);
                TcpSocketsUtils.Send(_socket, recvInProgressBuf, OnSend, OnExcp);
            }


            if ((DateTime.UtcNow - _lastKeepAliveTime).TotalSeconds > _settings.KeepAliveDisconnectInterval)
            {
                if (isRecvActive == false)
                {
                    if (_socket != null && IsConnected)
                    {
                        OnDebugLog?.Invoke(DebugLogType.OnKeepAlive, "Need to Disconnect");
                        IsConnected = false;
                        try { _socket.Dispose(); } catch { }
                        _socket = null;
                        OnDisconnect?.Invoke();

                    }
                }
                else
                {
                    OnDebugLog?.Invoke(DebugLogType.OnKeepAlive, "Need to Disconnect, but RecvProgressTime/RecvTime is in less than 10 seconds");
                }
            }

            if (_socket == null || _socket.Connected == false) //todo: check also keep alive status
            {
                DisconnectInternal();
                if (_settings.AutoReconnect)
                {
                    ConnectInternal();
                }
            }
        }


        public void Dispose()
        {
            if (_isDisposed) return;
            _isDisposed = true;
            try
            {
                _reconnectTimer.Stop();
                _socket.Close();
            }
            catch { }
        }
    }
}
