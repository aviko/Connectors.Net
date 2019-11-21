using System;
using System.Collections.Generic;
using System.Text;

namespace TcpConnectors
{
    internal static class ConnectorsUtils
    {
        internal static object DeserializePacket(byte[] buf, Dictionary<Tuple<int, int>, Type> packetsMap, out byte module, out byte command)
        {
            return Deserialize(2, buf, packetsMap, out module, out command);
        }

        internal static byte[] SerializePacket(int module, int command, object packet)
        {
            return Serialize(2, module, command, packet);
        }

        internal static byte[] SerializeRequestMultiResponsesPacket(int module, int command, object packet)
        {
            return Serialize(2, module, command, packet);
        }

        //=============================================================================
        //0 - always 0
        //1 - requestType: 0:keep alive, 1: request response, 2: request multi responses
        //2-5 - requestId
        //6 - module
        //7 - command

        internal const byte RequestTypeKeepAlive = 1;
        internal const byte RequestTypeRequestResponse = 2;
        internal const byte RequestTypeRequestMultiResponses = 3;
        internal const byte RequestTypeRecvInProgress = 4;

        internal static object DeserializeRequestPacket(byte[] buf, Dictionary<Tuple<int, int>, Type> packetsMap, out byte requestType, out int requestId, out byte module, out byte command)
        {
            requestType = buf[1];
            requestId = BitConverter.ToInt32(buf, 2);

            return Deserialize(8, buf, packetsMap, out module, out command);
        }


        internal static byte[] SerializeRequestPacket(byte requestType, int module, int command, object packet, int requestId)
        {
            var buf = Serialize(8, module, command, packet);
            buf[1] = requestType;
            var requestIdArr = BitConverter.GetBytes(requestId);
            Array.Copy(requestIdArr, 0, buf, 2, 4);
            return buf;
        }
        //=============================================================================

        private static object Deserialize(int offset, byte[] buf, Dictionary<Tuple<int, int>, Type> packetsMap, out byte module, out byte command)
        {
            var destBuf = new byte[buf.Length - offset];
            Array.Copy(buf, offset, destBuf, 0, buf.Length - offset);
            module = buf[offset - 2];
            command = buf[offset - 1];

            packetsMap.TryGetValue(new Tuple<int, int>(module, command), out var type);

            var packet = BinaryConverter.BinaryConvert.DeserializeObject(type, destBuf);
            return packet;
        }


        private static byte[] Serialize(int offset, int module, int command, object packet)
        {
            var type = packet == null ? typeof(object) : packet.GetType();

            //Console.WriteLine($"ConnectorsUtils.Serialize type={type.Name}");

            byte[] payloadBuf = BinaryConverter.BinaryConvert.SerializeObject(type, packet);
            byte[] output = new byte[offset + payloadBuf.Length];

            output[offset - 2] = (byte)module;
            output[offset - 1] = (byte)command;
            Array.Copy(payloadBuf, 0, output, offset, payloadBuf.Length);
            return output;
        }
    }
}
