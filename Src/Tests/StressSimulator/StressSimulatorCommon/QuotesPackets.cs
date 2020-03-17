using System;
using System.Collections.Generic;
using System.Text;
using TcpConnectors;

namespace StressSimulatorCommon
{
    public class UpdateQuotePackets : SendPacketsUtils.IServer2ClientPacket
    {
        public const byte MODULE = 3;
        public const byte COMMAND = 1;

        public string Symbol { get; set; }
        public decimal Bid { get; set; }
        public decimal Ask { get; set; }
    }
}
