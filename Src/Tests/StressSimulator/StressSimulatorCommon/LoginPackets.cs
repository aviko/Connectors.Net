using System;
using TcpConnectors;

namespace StressSimulatorCommon
{
    public class LoginRequestPacket : SendPacketsUtils.IClient2ServerPacket
    {
        public const byte MODULE = 1;
        public const byte COMMAND = 1;

        public string Username { get; set; }
    }

    public class LoginResponsePacket : SendPacketsUtils.IServer2ClientPacket
    {
        public const byte MODULE = 1;
        public const byte COMMAND = 1;

        public bool RetCode { get; set; }
        public string Message { get; set; }
    }
}
