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
            Console.WriteLine("Enter message");
            var line = Console.ReadLine();
            var msgPacket = new SendGroupMessagePacket()
            {
                Message = line
            };
            Program._clientConnector.Send(3, 1, msgPacket);
        }

        private void ActionSendPrivateMessage()
        {

        }

        private void ActionCreateGroup()
        {

        }

        private void ActionJoinGroup()
        {

        }

        private void ActionLeaveGroup()
        {

        }

        private void ActionGetGroups()
        {

        }
        private void ActionGetGroupInfo()
        {

        }
    }
}
