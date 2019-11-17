using System;
using System.Collections.Generic;

namespace TestRequestMultiResponseCommon
{

    public class GetListRequestPacket
    {
        public const byte MODULE = 1;
        public const byte COMMAND = 1;
    }

    public class GetListResponsePacket 
    {
        public const byte MODULE = 1;
        public const byte COMMAND = 1;

        public List<string> List { get; set; }
    }
}
