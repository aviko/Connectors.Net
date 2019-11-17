using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace TcpConnectors.TestCommon
{
    //public interface IServer2ClientPacket
    //{
    //}

    //----------------------------------------------------

    public class LoginResponsePacket : SendPacketsUtils.IServer2ClientPacket
    {
        public const byte MODULE = 1;
        public const byte COMMAND = 1;

        public bool RetCode { get; set; }
        public string Message { get; set; }
    }

    //----------------------------------------------------
    //Groups


    public class CreateGroupResponsePacket : SendPacketsUtils.IServer2ClientPacket
    {
        public const byte MODULE = CreateGroupRequestPacket.MODULE;
        public const byte COMMAND = CreateGroupRequestPacket.COMMAND;

        public bool RetCode { get; set; }
        public string Message { get; set; }
    }

    public class JoinGroupResponsePacket : SendPacketsUtils.IServer2ClientPacket
    {
        public const byte MODULE = JoinGroupRequestPacket.MODULE;
        public const byte COMMAND = JoinGroupRequestPacket.COMMAND;

        public bool RetCode { get; set; }
        public string Message { get; set; }
    }

    public class LeaveGroupResponsePacket : SendPacketsUtils.IServer2ClientPacket
    {
        public const byte MODULE = 2;
        public const byte COMMAND = 3;

        public bool RetCode { get; set; }
        public string Message { get; set; }
    }

    public class GetGroupsResponsePacket : SendPacketsUtils.IServer2ClientPacket
    {
        public const byte MODULE = GetGroupsRequestPacket.MODULE;
        public const byte COMMAND = GetGroupsRequestPacket.COMMAND;

        public List<string> GroupNames { get; set; }
    }

    public class GetGroupInfoResponsePacket : SendPacketsUtils.IServer2ClientPacket
    {
        public const byte MODULE = 2;
        public const byte COMMAND = 5;

        public bool RetCode { get; set; }
        public string Message { get; set; }

        public string GroupName { get; set; }
        public string Admin { get; set; }
        public List<string> Members { get; set; }
    }

    //----------------------------------------------------
    //messages
    public class OnMessagePacket : SendPacketsUtils.IServer2ClientPacket
    {
        public const byte MODULE = 3;
        public const byte COMMAND = 1;

        public string From { get; set; }
        public string GroupName { get; set; } //can be empty if private
        public string Message { get; set; }
    }


    public class OnNotificationPacket : SendPacketsUtils.IServer2ClientPacket
    {
        public const byte MODULE = 3;
        public const byte COMMAND = 2;

        public string NotificationType { get; set; }
        public string Message { get; set; }
    }

    //----------------------------------------------------

}
