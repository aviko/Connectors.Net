using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

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

        private BlockingRequestResponseHandler<int, object> _reqResHandler = new BlockingRequestResponseHandler<int, object>();
        private AsyncRequestResponseHandler<int, object> _reqResAsyncHandler = new AsyncRequestResponseHandler<int, object>();


        private void OnRecv(byte[] buf)
        {
            // module, command
            if (buf[0] == 0) //request response packet
            {
                var reqPacket = ConnectorsUtils.DeserializeRequestPacket(buf, _settings.PacketsMap, out var requestId);
                if (buf[1] == 0 && requestId == 0) //keep alive
                {
                    _lastKeepAliveTime = DateTime.UtcNow;
                    TcpSocketsUtils.Send(_socket, buf, OnSend, OnExcp);
                    Console.WriteLine($"keep alive: {reqPacket}");
                    return;
                }

                if (requestId % 2 == 1)
                {
                    _reqResHandler.HandleResponse(requestId, reqPacket);
                }
                else
                {
                    _reqResAsyncHandler.HandleResponse(requestId, reqPacket);
                }
            }
            else //packet
            {
                object packet = ConnectorsUtils.DeserializePacket(buf, _settings.PacketsMap);
                OnPacket?.Invoke(buf[0], buf[1], packet);
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
            OnDebugLog?.Invoke(DebugLogType.OnSend, "");

        }

        private bool _isInConnectInternal = false;
        private void ConnectInternal()
        {
            try
            {
                //start keepAlive timer only on first connect
                if (_reconnectTimer == null)
                {
                    _reconnectTimer = new System.Timers.Timer(_settings.ReconnectInterval * 10);
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

        private void ReconnectTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
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
    }
}
