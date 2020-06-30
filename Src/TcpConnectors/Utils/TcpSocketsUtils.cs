using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace TcpConnectors.Utils
{
    public static class TcpSocketsUtils
    {

        //public const int ms_DefualtReceiveBufferSize = 5_000_000;
        public const int ms_DefualtReceiveBufferSize = 1_000_000;

        //----------- Send Section ---------------
        public delegate void OnSendDlgt(int dataSentBytes, int grossSentBytes);
        private class SendState
        {
            public long m_sendId;
            public Socket m_socket;
            public OnSendDlgt m_onSend;
            public OnExcpDlgt m_onExcp;
            public byte[] m_buf;
            public int m_bytesSentSoFar;
            public int m_bytesLenBuf;
            public DateTime m_startSendTime;
        }

        private static void ConvertIntToArray(int Val, out byte[] OutArray)
        {
            byte[] MaxNum = new byte[MaxNumOfBytesInLengthField];
            int ArrayLen = 0;

            do
            {
                MaxNum[ArrayLen++] = (byte)(Val & 0x7F);
                Val >>= 7;
            } while (Val != 0);

            OutArray = new byte[ArrayLen];
            int i;
            for (i = 0; i < ArrayLen; i++)
            {
                OutArray[i] = (byte)(MaxNum[ArrayLen - i - 1] | (byte)((i == (ArrayLen - 1)) ? 0x0 : 0x80));
            }
        }

        private static long _sendId = 1;

        // Send a string on the given socket.
        // The onSocket delegate will be executed once the send action has ended
        public static void Send(Socket socket, byte[] bufferToSend, OnSendDlgt onSend, OnExcpDlgt onExcp)
        {
            try
            {
                byte[] lenBuf;

                ConvertIntToArray(bufferToSend.Length, out lenBuf);

                SendState state = new SendState();
                state.m_sendId = Interlocked.Increment(ref _sendId);
                state.m_socket = socket;
                state.m_onSend = onSend;
                state.m_onExcp = onExcp;
                state.m_buf = new byte[lenBuf.Length + bufferToSend.Length];
                state.m_bytesLenBuf = lenBuf.Length;
                state.m_bytesSentSoFar = 0;
                state.m_startSendTime = DateTime.UtcNow;
                Buffer.BlockCopy(lenBuf, 0, state.m_buf, 0, lenBuf.Length);
                Buffer.BlockCopy(bufferToSend, 0, state.m_buf, lenBuf.Length, bufferToSend.Length);

                //Begin sending the data to the remote device.
                var asyncResult = state.m_socket.BeginSend(
                    state.m_buf,
                    0,
                    state.m_buf.Length,
                    0,
                    new AsyncCallback(SendCallback),
                    state);


                //var isSignaled = asyncResult.AsyncWaitHandle.WaitOne(0);
                //if (isSignaled == false)
                //{
                //    ThreadPool.RegisterWaitForSingleObject(asyncResult.AsyncWaitHandle, SendCompleteCallback, state, -1, true);
                //    Console.WriteLine($"* asyncResult: IsCompleted:{asyncResult.IsCompleted} socket Handle: {state.m_socket.Handle.ToInt32()} sendId: #{state.m_sendId}");
                //}

            }
            catch (ObjectDisposedException)
            {
                // DisposedException is received if the current host initiated
                // a disconnection. In this case - do nothing.
            }
            catch (Exception e)
            {
                onExcp(e);
            }
        }

        //private static void SendCompleteCallback(object stateObj, bool timedOut)
        //{
        //    SendState state = (SendState)stateObj;
        //    var timespan = DateTime.UtcNow - state.m_startSendTime;
        //    Console.WriteLine($"* SendCompleteCallback ms:{timespan.TotalMilliseconds:0,0.00} socket Handle: {state.m_socket.Handle.ToInt32()} sendId: #{state.m_sendId}");
        //}

        private static void SendCallback(IAsyncResult ar)
        {
            SendState state = (SendState)ar.AsyncState;
            try
            {
                if (state.m_socket.Connected == false)
                {
                    return;
                }

                // Complete sending the data to the remote device.
                int bytesSent = state.m_socket.EndSend(ar);
                state.m_bytesSentSoFar += bytesSent;
                //Console.WriteLine($"SendCallback m_BytesSentSoFar:{state.m_BytesSentSoFar} Length:{state.m_buf.Length}");
                if (state.m_bytesSentSoFar < state.m_buf.Length)
                {
                    //keep sending
                    state.m_socket.BeginSend(
                        state.m_buf,
                        state.m_bytesSentSoFar,
                        state.m_buf.Length - state.m_bytesSentSoFar,
                        0,
                        new AsyncCallback(SendCallback),
                        state);

                }
                else
                {
                    //var timespan = DateTime.UtcNow - state.m_startSendTime;
                    //if (timespan.Milliseconds > 10)
                    //{
                    //    Console.WriteLine($"* send timespan is high ms:{timespan.TotalMilliseconds:0,0.00}  socket Handle: {state.m_socket.Handle.ToInt32()} sendId: #{state.m_sendId}");
                    //}

                    // Signal that all bytes have been sent.
                    state.m_onSend(state.m_buf.Length - state.m_bytesLenBuf, state.m_buf.Length);
                }

            }
            catch (ObjectDisposedException)
            {
                // DisposedException is received if the current host initiated
                // a disconnection. In this case - do nothing.
            }
            catch (Exception e)
            {
                state.m_onExcp(e);
            }
        }

        //----------- Recv Section ---------------

        /// <summary>
        /// This delegate will be called upon a new buffer arrived.
        /// NOTE: you MUSTN'T do any heavy action in your called method.
        /// </summary>
        /// <param name="buf"></param>
        public delegate void OnRecvDlgt(byte[] buf, int grossRecvBytes);
        public delegate void OnRecvProgressDlgt(int bytesRecived, int totalPacketLen);
        public delegate void OnExcpDlgt(Exception e);

        private const int MaxNumOfBytesInLengthField = 5;

        private class RecvState
        {
            public Socket m_Socket;
            public OnRecvDlgt m_OnRecv;
            public OnRecvProgressDlgt m_OnRecvProgress;
            public OnExcpDlgt m_OnExcp;
            public byte[] m_buf;
            public byte[] m_PrevLeftoverBuf;
            public bool m_recvLoop;
            public int m_bytesLenBuf;
        }

        public static void Recv(Socket socket, OnRecvDlgt onRecv, OnExcpDlgt onExcp, OnRecvProgressDlgt onRecvProgress, int reciveBufferSize, bool recvLoop)
        {
            try
            {
                // Create the state object.
                RecvState state = new RecvState();
                state.m_Socket = socket;
                state.m_OnRecv = onRecv;
                state.m_OnRecvProgress = onRecvProgress;
                state.m_OnExcp = onExcp;
                state.m_recvLoop = recvLoop;
                state.m_buf = (recvLoop ? new byte[reciveBufferSize] : new byte[1]);
                StartRecvPacket(state);
            }
            catch (Exception e)
            {
                if (!(e is System.ObjectDisposedException))
                {

                }
            }
        }

        //
        // NOTE NOTE NOTE:
        //
        // BeginReceive() receives a callback which will be called upon completion of the action.
        // The callback might be called from a different thread - OR -
        // it can be called in this thread from within the BeginReceive() function.
        // Keep that in mind: You should not change or rely on any state after you call this function,
        // otherwise you might get some serious bugs in your system.
        //
        private static void StartRecvPacket(RecvState state)
        {
            // Begin receiving the data from the remote device, read buf len
            if (state.m_Socket.Connected)
            {
                state.m_Socket.BeginReceive(
                    state.m_buf,
                    0, //offset
                    state.m_buf.Length, //len of len (start with 1 byte, if needed we'll read more later)
                    0, //flags
                    new AsyncCallback(RecvCallbackFromDevice),
                    state);
            }
        }

        private static void RecvCallbackFromDevice(IAsyncResult ar)
        {
            RecvState state = (RecvState)ar.AsyncState;
            try
            {
                // Retrieve the state object and the client socket 
                // from the asynchronous state object.

                // can get the disconnected socket status
                if (state.m_Socket.Connected == false)
                {
                    return;
                }
                // Read data from the remote device.
                int bytesRead = state.m_Socket.EndReceive(ar);
                ParseBuffer(state, bytesRead);
            }
            catch (ObjectDisposedException)
            {
                // DisposedException is received if the current host initiated
                // a disconnection. In this case - do nothing.
            }
            catch (Exception e)
            {
                state.m_OnExcp(e);
            }
        }

        private static void ParseBuffer(RecvState state, int dataLength)
        {
            if (dataLength == 0)
            {
                return;
            }
            byte[] workingBuf;
            if (state.m_PrevLeftoverBuf != null)
            {
                workingBuf = new byte[dataLength + state.m_PrevLeftoverBuf.Length];
                Buffer.BlockCopy(state.m_PrevLeftoverBuf, 0, workingBuf, 0, state.m_PrevLeftoverBuf.Length);
                Buffer.BlockCopy(state.m_buf, 0, workingBuf, state.m_PrevLeftoverBuf.Length, dataLength);
                state.m_PrevLeftoverBuf = null;
                dataLength = workingBuf.Length;
            }
            else
            {
                workingBuf = state.m_buf;
            }

            int expectedTotalPacketLen = 0;
            int packetLenBytes = 0;

            for (int i = 0; i < dataLength;)
            {
                /********************************************************************/
                // Read packet length
                /********************************************************************/
                packetLenBytes = 1;
                state.m_bytesLenBuf = 1;
                expectedTotalPacketLen = workingBuf[i] & 0x7F;
                while ((workingBuf[i] & 0x80) == 0x80)
                {
                    i++;
                    if (i < dataLength)
                    {
                        expectedTotalPacketLen <<= 7;
                        expectedTotalPacketLen += workingBuf[i] & 0x7F;
                        packetLenBytes++;
                        state.m_bytesLenBuf = packetLenBytes;
                    }
                    else
                    {
                        state.m_PrevLeftoverBuf = new byte[packetLenBytes];
                        Buffer.BlockCopy(workingBuf, i - packetLenBytes, state.m_PrevLeftoverBuf, 0, packetLenBytes);
                        StartRecvPacket(state);
                        return;
                    }
                }
                /********************************************************************/
                // End Read packet length
                /********************************************************************/
                i++;
                int packetBytesRecived = Math.Min(expectedTotalPacketLen, dataLength - i);
                if (packetBytesRecived < expectedTotalPacketLen)
                {
                    state.m_PrevLeftoverBuf = new byte[packetBytesRecived + packetLenBytes];
                    Array.Copy(workingBuf, i - packetLenBytes, state.m_PrevLeftoverBuf, 0, packetBytesRecived + packetLenBytes);
                    if (!state.m_recvLoop)
                    {
                        state.m_buf = new byte[expectedTotalPacketLen - packetBytesRecived];
                    }
                    state.m_OnRecvProgress?.Invoke(packetBytesRecived, expectedTotalPacketLen);
                    StartRecvPacket(state);

                    return;
                }

                byte[] data = new byte[packetBytesRecived];
                Array.Copy(workingBuf, i, data, 0, packetBytesRecived);
                state.m_OnRecv(data, data.Length + state.m_bytesLenBuf);
                i += packetBytesRecived;
            }
            if (state.m_recvLoop)
            {
                StartRecvPacket(state);
            }
        }

        //----------- Connect Section ---------------
        public static Socket Connect(string addr, int port)
        {
            // Establish the remote endpoint for the socket.
            IPAddress ipAddress;

            if (IPAddress.TryParse(addr, out ipAddress) == false)
            {
                IPHostEntry ipHostInfo = Dns.GetHostEntry(addr);
                ipAddress = ipHostInfo.AddressList[0];
            }

            IPEndPoint ep = new IPEndPoint(ipAddress, port);

            // Create a TCP/IP socket.
            Socket socket = new Socket(
                ipAddress.AddressFamily,
                SocketType.Stream,
                ProtocolType.Tcp);

            // Connect to the remote endpoint.
            socket.Connect(ep);
            return socket;
        }
    }
}
