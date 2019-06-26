using System;
using System.Collections.Generic;
using System.Text;

namespace TcpConnectors
{
    internal static class ConnectorsUtils
    {
        internal static object DeserializePacket(byte[] buf, Dictionary<Tuple<int, int>, Type> typeMap)
        {
            return Deserialize(2, buf, typeMap);
        }

        internal static byte[] SerializePacket(int module, int command, object packet)
        {
            return Serialize(2, module, command, packet);
        }

        internal static object DeserializeRequestPacket(byte[] buf, Dictionary<Tuple<int, int>, Type> typeMap, out int requestId)
        {
            requestId = BitConverter.ToInt32(buf, 1);
            return Deserialize(7, buf, typeMap);
        }

        internal static byte[] SerializeRequestPacket(int module, int command, object packet, int requestId)
        {
            var buf = Serialize(7, module, command, packet);
            var requestIdArr = BitConverter.GetBytes(requestId);
            Array.Copy(requestIdArr, 0, buf, 1, 4);
            return buf;
        }

        private static object Deserialize(int offset, byte[] buf, Dictionary<Tuple<int, int>, Type> typeMap)
        {
            var destBuf = new byte[buf.Length - offset];
            Array.Copy(buf, offset, destBuf, 0, buf.Length - offset);

            typeMap.TryGetValue(new Tuple<int, int>(buf[offset - 2], buf[offset - 1]), out var type);

            var packet = BinaryConverter.BinaryConvert.DeserializeObject(type, destBuf);
            return packet;
        }


        private static byte[] Serialize(int offset, int module, int command, object packet)
        {
            byte[] payloadBuf = BinaryConverter.BinaryConvert.SerializeObject(packet.GetType(), packet);
            byte[] output = new byte[offset + payloadBuf.Length];

            output[offset - 2] = (byte)module;
            output[offset - 1] = (byte)command;
            Array.Copy(payloadBuf, 0, output, offset, payloadBuf.Length);
            return output;
        }
    }
}
