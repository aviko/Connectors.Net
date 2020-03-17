using System;
using System.Collections.Generic;
using System.Text;
using TcpConnectors;

namespace StressSimulatorCommon
{
    public class ReportRequestPacket : SendPacketsUtils.IClient2ServerPacket
    {
        public const byte MODULE = 2;
        public const byte COMMAND = 1;

        public string ReportName { get; set; }
    }

    public class ReportResponsePacket : SendPacketsUtils.IServer2ClientPacket
    {
        public const byte MODULE = 2;
        public const byte COMMAND = 1;

        public List<string> Records { get; set; }
    }
}
