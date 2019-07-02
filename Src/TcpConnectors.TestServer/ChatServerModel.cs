using System;
using System.Collections.Generic;
using System.Text;

namespace TcpConnectors.TestServer
{

    public class ChatGroup
    {
        public string GroupName { get; set; }
        public string Admin { get; set; }
        public List<string> Members { get; set; } = new List<string>();
    }

    public class ChatServerModel
    {
        public ChatServerModel()
        {
            Groups.Add("Lobby", new ChatGroup() { GroupName = "Lobby" });
        }

        public Dictionary<string, ChatGroup> Groups { get; set; } = new Dictionary<string, ChatGroup>();
        public HashSet<string> Users { get; set; } = new HashSet<string>();


        internal string CreateGroup(string groupName, string creatorName)
        {
            if (string.IsNullOrEmpty(groupName))
            {
                return "GroupName empty";
            }

            if (Groups.TryGetValue(groupName, out var chatGroup))
            {
                return $"GroupName {groupName} already exist";
            }

            var newGroup = new ChatGroup { GroupName = groupName };
            newGroup.Members.Add(creatorName);
            Groups.Add(groupName, newGroup);

            return null;
        }
    }
}
