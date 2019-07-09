using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using TcpConnectors.TestCommon;

namespace TcpConnectors.TestClient
{
    class MenusHandler
    {
        internal void TopLevelMenu()
        {
            while (true)
            {
                ConsoleUI.ShowMenu("Select Action",
                    ActionExit,
                    ActionSendGroupMessage,
                    ActionSendPrivateMessage,
                    ActionCreateGroup,
                    ActionJoinGroup,
                    ActionLeaveGroup,
                    ActionGetGroups,
                    ActionGetGroupInfo);
            }
        }

        private void ActionExit()
        {
            Environment.Exit(0);
        }

        private void ActionSendGroupMessage()
        {
            var groups = Program._clientConnector.SendRequest(GetGroupsRequestPacket.MODULE, GetGroupsRequestPacket.COMMAND, new GetGroupsRequestPacket { }) as GetGroupsResponsePacket;

            var groupName = ConsoleUI.ShowMenu("Select Group", groups.GroupNames.ToArray());

            Console.WriteLine("Enter message");
            var line = Console.ReadLine();
            var msgPacket = new SendGroupMessagePacket()
            {
                GroupName = groupName,
                Message = line
            };
            Program._clientConnector.Send(SendGroupMessagePacket.MODULE, SendGroupMessagePacket.COMMAND, msgPacket);
        }

        private void ActionSendPrivateMessage()
        {

            Console.WriteLine("Enter username");
            var username = Console.ReadLine();

            Console.WriteLine("Enter message");
            var line = Console.ReadLine();
            var msgPacket = new SendPrivateMessagePacket()
            {
                Username = username,
                Message = line
            };
            Program._clientConnector.Send(SendPrivateMessagePacket.MODULE, SendPrivateMessagePacket.COMMAND, msgPacket);
        }

        private void ActionCreateGroup()
        {
            Console.WriteLine("Enter new Group Name");
            var line = Console.ReadLine();
            var res = Program._clientConnector.SendRequest(CreateGroupRequestPacket.MODULE, CreateGroupRequestPacket.COMMAND, new CreateGroupRequestPacket { GroupName = line });
            Console.WriteLine($"Response: { JsonConvert.SerializeObject(res)}");
        }

        private void ActionJoinGroup()
        {
            Console.WriteLine("Enter new Group Name");
            var line = Console.ReadLine();
            var res = Program._clientConnector.SendRequest(JoinGroupRequestPacket.MODULE, JoinGroupRequestPacket.COMMAND, new JoinGroupRequestPacket { GroupName = line });
            Console.WriteLine($"Response: { JsonConvert.SerializeObject(res)}");
        }

        private void ActionLeaveGroup()
        {
            Console.WriteLine("Enter new Group Name");
            var line = Console.ReadLine();
            var res = Program._clientConnector.SendRequest(LeaveGroupRequestPacket.MODULE, LeaveGroupRequestPacket.COMMAND, new LeaveGroupRequestPacket { GroupName = line });
            Console.WriteLine($"Response: { JsonConvert.SerializeObject(res)}");
        }

        private void ActionGetGroups()
        {
            var res = Program._clientConnector.SendRequest(GetGroupsRequestPacket.MODULE, GetGroupsRequestPacket.COMMAND, new GetGroupsRequestPacket { });
            Console.WriteLine($"Response: { JsonConvert.SerializeObject(res)}");
        }

        private void ActionGetGroupInfo()
        {
            Console.WriteLine("Enter new Group Name");
            var line = Console.ReadLine();
            var res = Program._clientConnector.SendRequest(GetGroupInfoRequestPacket.MODULE, GetGroupInfoRequestPacket.COMMAND, new GetGroupInfoRequestPacket { GroupName = line });
            Console.WriteLine($"Response: { JsonConvert.SerializeObject(res)}");
        }
    }
}
