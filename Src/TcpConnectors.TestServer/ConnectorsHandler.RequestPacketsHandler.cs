using System;
using System.Collections.Generic;
using System.Text;
using TcpConnectors.TestCommon;

namespace TcpConnectors.TestServer
{
    partial class ConnectorsHandler
    {

        internal object HandleRequestPacket(ServerConnectorContext serverConnectorContext, int module, int command, dynamic packet)
        {
            Console.WriteLine("Warning HandlePacket - type object");
            return null;
        }

        internal object HandleRequestPacket(ServerConnectorContext serverConnectorContext, int module, int command, LoginRequestPacket packet)
        {
            var resPacket = new LoginResponsePacket()
            {
                Username = packet.Username
            };

            return resPacket;
        }

        internal object HandleRequestPacket(ServerConnectorContext serverConnectorContext, int module, int command, CreateGroupRequestPacket packet)
        {
            string errMsg = Program.ChatServerModel.CreateGroup(serverConnectorContext.Id.ToString(), packet.GroupName);

            var resPacket = new CreateGroupResponsePacket()
            {
                IsCreated = errMsg == null,
                Message = errMsg ?? $"Group {packet.GroupName} Created OK",
            };

            return   resPacket;
        }


    }
}
