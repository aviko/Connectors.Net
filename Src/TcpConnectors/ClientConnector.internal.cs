using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace TcpConnectors
{
    partial class ClientConnector
    {
        private Socket _socket = null;
        private ClientConnectorSettings _settings;
        private int _nextRequestId = 1; //odd numbers
        private int _nextRequestIdAsync = 2; //even numbers
        private DateTime _lastKeepAliveTime = DateTime.UtcNow;
        private System.Timers.Timer _reconnectTimer = null;
        private bool _isDisposed = false;

        private BlockingRequestResponseHandler<int, object> _reqResHandler = new BlockingRequestResponseHandler<int, object>();
        private AsyncRequestResponseHandler<int, object> _reqResAsyncHandler = new AsyncRequestResponseHandler<int, object>();

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
                // module, command
                if (buf[0] == 0) //request response packet
                {
                    object reqPacket = null;
                    int requestId = 0;
                    byte module = 0, command = 0;
                    try
                    {
                        reqPacket = ConnectorsUtils.DeserializeRequestPacket(buf, _settings.PacketsMap, out requestId, out module, out command);
                    }
                    catch (Exception ex) { OnDebugLog?.Invoke(DebugLogType.OnRecvException, ex.ToString()); }

                    if (command == 0 && requestId == 0) //keep alive
                    {
                        _lastKeepAliveTime = DateTime.UtcNow;
                        TcpSocketsUtils.Send(_socket, buf, OnSend, OnExcp);
                        OnDebugLog?.Invoke(DebugLogType.OnKeepAlive, reqPacket.ToString());
                        return;
                    }

                    if (requestId % 2 == 1)
                    {
                        if (module == 0 && command == 1)
                        {
                            _reqResHandler.HandleExceptionResponse(requestId, new Exception((reqPacket ?? "").ToString()));
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
                            _reqResAsyncHandler.HandleExceptionResponse(requestId, new Exception((reqPacket ?? "").ToString()));
                        }
                        else
                        {
                            _reqResAsyncHandler.HandleResponse(requestId, reqPacket);
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
                if (_socket != null)
                {
                    IsConnected = true;
                    OnConnect?.Invoke();
                    TcpSocketsUtils.Recv(
                        _socket,
                        OnRecv,
                        OnExcp,
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

            if ((DateTime.UtcNow - _lastKeepAliveTime).TotalSeconds > _settings.KeepAliveDisconnectInterval)
            {
                if (_socket != null && IsConnected)
                {
                    IsConnected = false;
                    try { _socket.Dispose(); } catch { }
                    _socket = null;
                    OnDisconnect?.Invoke();

                }
            }

            if (_socket == null || _socket.Connected == false) //todo: chekc also keep alive status
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
