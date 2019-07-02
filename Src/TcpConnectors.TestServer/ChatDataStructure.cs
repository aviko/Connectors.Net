using System;
using System.Collections.Generic;
using System.Text;

namespace TcpConnectors.TestServer
{

    public class ChatGroup
    {
        public string GroupName { get; set; }
        public string Admin { get; set; }
        public List<string> Members { get; set; }
    }

    public class ChatManager
    {
        public Dictionary<string, ChatGroup> Groups { get; set; }
        public HashSet<string> Users { get; set; }
    }
}
