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

        private System.Timers.Timer _keepAliveTimer = null;

        private BlockingRequestResponseHandler<int, object> _reqResHandler = new BlockingRequestResponseHandler<int, object>();
        private AsyncRequestResponseHandler<int, object> _reqResAsyncHandler = new AsyncRequestResponseHandler<int, object>();


        private void OnRecv(byte[] buf)
        {
            if (buf[0] == 0) //request response packet
            {
                var reqPacket = ConnectorsUtils.DeserializeRequestPacket(buf, _settings.TypesMap, out var requestId);
                if (buf[1] == 0 && requestId == 0) //keep alive
                {
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
                object packet = ConnectorsUtils.DeserializePacket(buf, _settings.TypesMap);
                OnPacket?.Invoke(buf[0], buf[1], packet);
            }

        }

        private void OnExcp(Exception e)
        {
            if (_socket != null && _socket.Connected == false)
            {
                DisconnectInternal();
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
            Console.WriteLine("ClientConnector.OnSend");
        }

        private bool _isInConnectInternal = false;
        private void ConnectInternal()
        {
            try
            {
                //start keepAlive timer only on first connect
                if (_keepAliveTimer == null)
                {
                    _keepAliveTimer = new System.Timers.Timer(10_000);
                    _keepAliveTimer.Elapsed += KeepAliveTimer_Elapsed; ;
                    _keepAliveTimer.Start();
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
                        //todo: log
                    }
                }
                if (_socket != null)
                {
                    IsConnected = true;
                    OnConnect?.Invoke();
                    TcpSocketsUtils.Recv(_socket, OnRecv, OnExcp, TcpSocketsUtils.ms_DefualtReceiveBufferSize, true);
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

        private void KeepAliveTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
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
