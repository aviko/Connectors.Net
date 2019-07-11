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
        public Dictionary<string, HashSet<int>> Users { get; set; } = new Dictionary<string, HashSet<int>>();

        internal string Login(string username, ServerConnectorContext serverConnectorContext)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                return "username is empty";
            }

            if (Users.TryGetValue(username, out var connectorsIds) == false)
            {
                connectorsIds = new HashSet<int>();
                Users[username] = connectorsIds;
            }
            connectorsIds.Add(serverConnectorContext.Id); //user can be connected multiple times
            serverConnectorContext.Data = username;

            return null;
        }

        internal void Logout(string username, ServerConnectorContext serverConnectorContext)
        {
            if (Users.TryGetValue(username, out var connectorsIds))
            {
                connectorsIds.Remove(serverConnectorContext.Id);
            }
        }

        internal string CreateGroup(string groupName, string userName)
        {
            if (string.IsNullOrEmpty(groupName))
            {
                return "GroupName empty";
            }

            if (Groups.TryGetValue(groupName, out var chatGroup))
            {
                throw new Exception($"GroupName {groupName} already exist");
                //return $"GroupName {groupName} already exist";
            }

            var newGroup = new ChatGroup { GroupName = groupName };
            newGroup.Members.Add(userName);
            Groups.Add(groupName, newGroup);

            return null;
        }

        internal string JoinGroup(string groupName, string userName)
        {
            if (string.IsNullOrEmpty(groupName))
            {
                return "GroupName empty";
            }

            if (Groups.TryGetValue(groupName, out var chatGroup) == false)
            {
                return $"GroupName {groupName} not exist";
            }

            if (chatGroup.Members.Contains(userName))
            {
                return $"User {userName} already in group {groupName}";
            }

            chatGroup.Members.Add(userName);

            return null;
        }

        internal string LeaveGroup(string groupName, string userName)
        {
            if (string.IsNullOrEmpty(groupName))
            {
                return "GroupName empty";
            }

            if (Groups.TryGetValue(groupName, out var chatGroup) == false)
            {
                return $"GroupName {groupName} not exist";
            }

            if (chatGroup.Members.Contains(userName) == false)
            {
                return $"User {userName} not in group {groupName}";
            }

            chatGroup.Members.Remove(userName);

            return null;
        }
    }
}
