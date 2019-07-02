using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace TcpConnectors.TestCommon
{
    public interface IServer2ClientPacket
    {
    }

    //----------------------------------------------------

    public class LoginResponsePacket : IServer2ClientPacket
    {
        public const byte MODULE =  1;
        public const byte COMMAND =  1;

        public string Username { get; set; }
    }

    //----------------------------------------------------
    //Groups
    public class CreateGroupResponsePacket : IServer2ClientPacket
    {
        public const byte MODULE =  2;
        public const byte COMMAND =  1;

        public string GroupName { get; set; }
    }

    public class JoinGroupResponsePacket : IServer2ClientPacket
    {
        public const byte MODULE =  2;
        public const byte COMMAND =  2;

        public string GroupName { get; set; }
    }

    public class LeaveGroupResponsePacket : IServer2ClientPacket
    {
        public const byte MODULE =  2;
        public const byte COMMAND =  3;

        public string GroupName { get; set; }
    }

    public class GetGroupsResponsePacket : IServer2ClientPacket
    {
        public const byte MODULE =  2;
        public const byte COMMAND =  4;
    }

    public class GetGroupInfoResponsePacket : IServer2ClientPacket
    {
        public const byte MODULE =  2;
        public const byte COMMAND =  5;
    }

    //----------------------------------------------------
    //messages
    public class OnMessagePacket : IServer2ClientPacket
    {
        public const byte MODULE =  3;
        public const byte COMMAND =  1;

        public string From { get; set; }
        public string GroupName { get; set; } //can be empty if private
        public string Message { get; set; }
    }


    public class OnNotificationPacket : IServer2ClientPacket
    {
        public const byte MODULE =  3;
        public const byte COMMAND =  2;

        public string NotificationType { get; set; }
        public string Message { get; set; }
    }

    //----------------------------------------------------

}
