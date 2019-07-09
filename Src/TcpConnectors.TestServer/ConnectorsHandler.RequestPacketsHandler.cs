using System;
using System.Collections.Generic;
using System.Linq;
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

            string errMsg = Program.ChatServerModel.Login(packet.Username, serverConnectorContext);

            var resPacket = new LoginResponsePacket()
            {
                RetCode = errMsg == null,
                Message = errMsg == null ? "Login OK" : errMsg
            };

            return resPacket;
        }

        //---------------------------------------------------------------------------------

        internal object HandleRequestPacket(ServerConnectorContext serverConnectorContext, int module, int command, CreateGroupRequestPacket packet)
        {
            string errMsg = Program.ChatServerModel.CreateGroup(packet.GroupName, serverConnectorContext.Id.ToString());

            var resPacket = new CreateGroupResponsePacket()
            {
                RetCode = errMsg == null,
                Message = errMsg ?? $"Group {packet.GroupName} Created OK",
            };

            return resPacket;
        }

        internal object HandleRequestPacket(ServerConnectorContext serverConnectorContext, int module, int command, GetGroupsRequestPacket packet)
        {
            var resPacket = new GetGroupsResponsePacket()
            {
                GroupNames = Program.ChatServerModel.Groups.Values.Select(x => x.GroupName).ToList()
            };

            return resPacket;
        }

        internal object HandleRequestPacket(ServerConnectorContext serverConnectorContext, int module, int command, JoinGroupRequestPacket packet)
        {
            string errMsg = Program.ChatServerModel.JoinGroup(packet.GroupName, serverConnectorContext.Id.ToString());

            var resPacket = new JoinGroupResponsePacket()
            {
                RetCode = errMsg == null,
                Message = errMsg ?? $"Join to {packet.GroupName} OK",
            };

            return resPacket;
        }

        internal object HandleRequestPacket(ServerConnectorContext serverConnectorContext, int module, int command, LeaveGroupRequestPacket packet)
        {
            string errMsg = Program.ChatServerModel.LeaveGroup(packet.GroupName, serverConnectorContext.Id.ToString());

            var resPacket = new LeaveGroupResponsePacket()
            {
                RetCode = errMsg == null,
                Message = errMsg ?? $"Left {packet.GroupName} OK",
            };

            return resPacket;
        }

        internal object HandleRequestPacket(ServerConnectorContext serverConnectorContext, int module, int command, GetGroupInfoRequestPacket packet)
        {
            Program.ChatServerModel.Groups.TryGetValue(packet.GroupName, out var group);

            var resPacket = new GetGroupInfoResponsePacket()
            {
                RetCode = group != null,

                Admin = group?.Admin,
                GroupName = group?.GroupName,
                Members = group?.Members,
            };

            return resPacket;
        }


    }
}
