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

        internal static object DeserializeRequestPacket(byte[] buf, Dictionary<Tuple<int, int>, Type> packetsMap, out int requestId, out byte module, out byte command)
        {
            requestId = BitConverter.ToInt32(buf, 1);

            return Deserialize(7, buf, packetsMap, out module, out command);
        }

        internal static byte[] SerializeRequestPacket(int module, int command, object packet, int requestId)
        {
            var buf = Serialize(7, module, command, packet);
            var requestIdArr = BitConverter.GetBytes(requestId);
            Array.Copy(requestIdArr, 0, buf, 1, 4);
            return buf;
        }

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
