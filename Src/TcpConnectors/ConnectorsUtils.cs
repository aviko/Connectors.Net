using System;
using System.Collections.Generic;
using System.Text;

namespace TcpConnectors
{
    internal static class ConnectorsUtils
    {
        internal static object DeserializePacket(byte[] buf, Dictionary<Tuple<int, int>, Type> typeMap)
        {
            var destBuf = new byte[buf.Length - 2];
            Array.Copy(buf, 2, destBuf, 0, buf.Length - 2);

            typeMap.TryGetValue(new Tuple<int, int>(buf[0], buf[1]), out var type);

            var packet = BinaryConverter.BinaryConvert.DeserializeObject(type, destBuf);
            return packet;
        }


        internal static byte[] SerializePacket(int module, int command, object packet)
        {
            byte[] payloadBuf = BinaryConverter.BinaryConvert.SerializeObject(packet.GetType(), packet.ToString());
            byte[] output = new byte[2 + payloadBuf.Length];

            output[0] = (byte)module;
            output[1] = (byte)command;
            Array.Copy(payloadBuf, 0, output, 2, payloadBuf.Length);
            return output;
        }
    }
}
