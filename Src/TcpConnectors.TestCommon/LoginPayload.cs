using System;

namespace TcpConnectors.TestCommon
{
    public class LoginPayload
    {
        public string Username { get; set; }
        public string EncPassword { get; set; }
        public byte GetCommandModule()
        {
            return (byte)1;
        }

        public byte GetCommand()
        {
            return (byte)1;
        }

        public override string ToString()
        {
            return $"Username:{Username} EncPassword:{EncPassword}";
        }
    }
}
