using System;
using System.Threading.Tasks;
using TcpConnectors.Utils;

namespace TcpConnectors
{
    partial class ServerConnectorContext
    {
        internal class RequestResponseData
        {
            internal int RequestId { get; set; }
            internal int Module { get; set; }
            internal int Command { get; set; }
            internal object Packet { get; set; }
        }

        private ServerConnectors _serverConnectors;
        internal long _lastReceivedKeepAliveTimestamp;
        internal DateTime _lastReceivedInProgressTime;
        internal DateTime _connectedTime = default(DateTime);

        internal ServerConnectorContext(int id, ServerConnectors serverConnectors)
        {
            Id = id;
            _serverConnectors = serverConnectors;
            _connectedTime = DateTime.UtcNow;
        }

        internal void OnRecv(byte[] buf)
        {
            try
            {
                _serverConnectors.TriggerOnDebugLog(this, DebugLogType.Info, "Recv stated");

                if (buf[0] == 0) //request response packet
                {
                    object reqPacket = null;
                    int requestId = 0;
                    string exceptionMsg = null;
                    byte module = 0, command = 0, requestType = 0;
                    try
                    {
                        requestType = buf[1];

                        _serverConnectors.TriggerOnDebugLog(this, DebugLogType.Info, $"Recv requestType = {requestType}");


                        //if (requestType != ConnectorsUtils.RequestTypeRequestMultiResponses)
                        {
                            reqPacket = ConnectorsUtils.DeserializeRequestPacket(buf, _serverConnectors._settings.PacketsMap, out requestId, out module, out command);
                        }
                    }
                    catch (Exception ex) { exceptionMsg = ex.Message; }


                    if (requestType == ConnectorsUtils.RequestTypeKeepAlive) //keep alive
                    {
                        _lastReceivedKeepAliveTimestamp = (long)reqPacket;
                        _serverConnectors.TriggerOnDebugLog(this, DebugLogType.OnKeepAlive, reqPacket.ToString());
                        return;
                    }

                    if (requestType == ConnectorsUtils.RequestTypeRecvInProgress) //client RecvInProgress (keep alive should be less sensitive)
                    {
                        _lastReceivedInProgressTime = DateTime.UtcNow;
                        _serverConnectors.TriggerOnDebugLog(this, DebugLogType.OnKeepAlive, "Recv in progress");
                        return;
                    }

                    if (requestType == ConnectorsUtils.RequestTypeRequestResponse)
                    {
                        if (exceptionMsg != null)
                        {
                            var resBuf = ConnectorsUtils.SerializeRequestPacket(ConnectorsUtils.RequestTypeRequestResponse, 0, 1, exceptionMsg, requestId);
                            TcpSocketsUtils.Send(Socket, resBuf, OnSend, OnExcp);
                        }
                        else
                        {

                            var rrData = new RequestResponseData()
                            {
                                RequestId = requestId,
                                Module = module,
                                Command = command,
                                Packet = reqPacket,
                            };

                            new Task(() => HandleRequestResponse(rrData)).Start();
                        }
                    }

                    if (requestType == ConnectorsUtils.RequestTypeRequestMultiResponses)
                    {
                        if (exceptionMsg != null)
                        {
                            var resBuf = ConnectorsUtils.SerializeMultiResponsePacket(
                                ConnectorsUtils.RequestTypeRequestMultiResponses, 0, 1, exceptionMsg, requestId,
                                true, 0, 0);
                            TcpSocketsUtils.Send(Socket, resBuf, OnSend, OnExcp);
                        }
                        else
                        {
                            //reqPacket = ConnectorsUtils.DeserializeMultiResponsePacket(
                            //    buf, _serverConnectors._settings.PacketsMap, out requestId,
                            //    out bool isLast, out int nReceived, out int nTotal,
                            //    out module, out command);

                            _serverConnectors.TriggerOnDebugLog(this, DebugLogType.Info, $"Recv RequestTypeRequestResponse requestId = {requestId}");

                            var rrData = new RequestResponseData()
                            {
                                RequestId = requestId,
                                Module = module,
                                Command = command,
                                Packet = reqPacket,
                            };
                            new Task(() => HandleRequestMultiResponses(rrData)).Start();
                        }
                    }


                }
                else //packet
                {
                    var packet = ConnectorsUtils.DeserializePacket(buf, _serverConnectors._settings.PacketsMap, out byte module, out byte command);
                    _serverConnectors.TriggerOnPacket(this, module, command, packet);
                }
            }
            catch (Exception ex)
            {
                _serverConnectors.TriggerOnDebugLog(this, DebugLogType.OnRecvException, ex.ToString());
            }
        }

        internal void OnRecvProgress(int bytesRecived, int totalPacketLen)
        {
            Console.WriteLine($"OnRecvProgress bytesRecived:{bytesRecived}, totalPacketLen:{totalPacketLen}");
        }

        private void HandleRequestResponse(RequestResponseData rrData)
        {
            try
            {
                var res = _serverConnectors.TriggerOnRequestPacket(this, rrData.Module, rrData.Command, rrData.Packet);
                var resBuf = ConnectorsUtils.SerializeRequestPacket(ConnectorsUtils.RequestTypeRequestResponse, rrData.Module, rrData.Command, res, rrData.RequestId);
                TcpSocketsUtils.Send(Socket, resBuf, OnSend, OnExcp);
            }
            catch (Exception ex)
            {
                _serverConnectors.TriggerOnDebugLog(this, DebugLogType.OnRecvException, $"Module={rrData.Module} Command={rrData.Command} ex={ex.ToString()}");
                var resBuf = ConnectorsUtils.SerializeRequestPacket(ConnectorsUtils.RequestTypeRequestResponse, 0, 1, ex.Message, rrData.RequestId);
                TcpSocketsUtils.Send(Socket, resBuf, OnSend, OnExcp);
            }
        }
        private void HandleRequestMultiResponses(RequestResponseData rrData)
        {
            try
            {
                _serverConnectors.TriggerOnRequestMultiResponsesPacket(this, rrData.Module, rrData.Command, rrData.RequestId, rrData.Packet, RequestMultiResponsesCallback);
            }
            catch (Exception ex)
            {
                _serverConnectors.TriggerOnDebugLog(this, DebugLogType.OnRecvException, $"Module={rrData.Module} Command={rrData.Command} ex={ex.ToString()}");
                var resBuf = ConnectorsUtils.SerializeMultiResponsePacket(ConnectorsUtils.RequestTypeRequestMultiResponses, 0, 1, ex.Message, rrData.RequestId, true, 0, 0);
                TcpSocketsUtils.Send(Socket, resBuf, OnSend, OnExcp);
            }
        }

        private void RequestMultiResponsesCallback(
            ServerConnectorContext serverConnectorContext, int module, int command, int requestId,
            object packet, bool isLast, int nReceived, int nTotal, Exception exception)
        {
            Console.WriteLine($"*********** RequestMultiResponsesCallback ************ requestId = {requestId}");

            var resBuf = ConnectorsUtils.SerializeMultiResponsePacket(
                ConnectorsUtils.RequestTypeRequestMultiResponses, module, command, packet, requestId, isLast, nReceived, nTotal);
            TcpSocketsUtils.Send(Socket, resBuf, OnSend, OnExcp);
        }

        internal void OnSend()
        {
            _serverConnectors.TriggerOnDebugLog(this, DebugLogType.OnSend, "");
            //Console.WriteLine("ServerConnectorContext.OnSend()");
        }

        internal void OnExcp(Exception ex)
        {
            if (Socket.Connected)
            {
                _serverConnectors.TriggerOnException(this, ex);
            }
            else
            {
                //not connected
                if (_serverConnectors._contextMap.TryRemove(this.Id, out var removed))
                {
                    _serverConnectors.TriggerOnDisconnect(this);
                }
            }

        }
    }
}
