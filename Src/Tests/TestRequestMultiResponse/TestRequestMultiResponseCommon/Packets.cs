using System;
using System.Collections.Generic;
using static TcpConnectors.SendPacketsUtils;

namespace TestRequestMultiResponseCommon
{

    public class GetListRequestPacket
    {
        public const byte MODULE = 1;
        public const byte COMMAND = 1;
    }

    public class GetListResponsePacket : IMultiResponseListPacket<string>
    {
        public const byte MODULE = 1;
        public const byte COMMAND = 1;

        public List<string> Records { get; set; }
    }
    public class GetListRequestMultiResponsesPacket
    {
        public const byte MODULE = 1;
        public const byte COMMAND = 2;
    }
}
