using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;


namespace TcpConnectors.TestCommon
{
    //public interface IClient2ServerPacket
    //{
    //}


    //----------------------------------------------------

    public class LoginRequestPacket : SendPacketsUtils.IClient2ServerPacket
    {
        public const byte MODULE =  1;
        public const byte COMMAND =  1;

        public string Username { get; set; }


    }

    //----------------------------------------------------
    //Groups
    public class CreateGroupRequestPacket : SendPacketsUtils.IClient2ServerPacket
    {
        public const byte MODULE =  2;
        public const byte COMMAND =  1;

        public string GroupName { get; set; }
    }

    public class JoinGroupRequestPacket : SendPacketsUtils.IClient2ServerPacket
    {
        public const byte MODULE =  2;
        public const byte COMMAND =  2;

        public string GroupName { get; set; }
    }

    public class LeaveGroupRequestPacket : SendPacketsUtils.IClient2ServerPacket
    {
        public const byte MODULE =  2;
        public const byte COMMAND =  3;

        public string GroupName { get; set; }
    }

    public class GetGroupsRequestPacket : SendPacketsUtils.IClient2ServerPacket
    {
        public const byte MODULE =  2;
        public const byte COMMAND =  4;
    }

    public class GetGroupInfoRequestPacket : SendPacketsUtils.IClient2ServerPacket
    {
        public const byte MODULE =  2;
        public const byte COMMAND =  5;

        public string GroupName { get; set; }
    }

    //----------------------------------------------------
    //messages
    public class SendGroupMessagePacket : SendPacketsUtils.IClient2ServerPacket
    {
        public const byte MODULE =  3;
        public const byte COMMAND =  1;

        public string GroupName { get; set; }
        public string Message { get; set; }
    }

    public class SendPrivateMessagePacket : SendPacketsUtils.IClient2ServerPacket
    {
        public const byte MODULE =  3;
        public const byte COMMAND =  2;

        public string Username { get; set; }
        public string Message { get; set; }
    }

    //----------------------------------------------------
    //


}
