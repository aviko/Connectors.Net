using System;
using System.Collections.Generic;
using System.Text;
using TcpConnectors.TestCommon;

namespace TcpConnectors.TestServer
{
    partial class ConnectorsHandler
    {
        internal void HandlePacket(ServerConnectorContext serverConnectorContext, int module, int command, dynamic packet)
        {
            Console.WriteLine("Warning HandlePacket - type object");
        }

        internal void HandlePacket(ServerConnectorContext serverConnectorContext, int module, int command, SendGroupMessagePacket packet)
        {
            var msgPacket = new OnMessagePacket()
            {
                Message = packet.Message,
            };

            _serverConnectors.Send(x => true, module, command, msgPacket);
        }

        internal void HandlePacket(ServerConnectorContext serverConnectorContext, int module, int command, SendPrivateMessagePacket packet)
        {
            var msgPacket = new OnMessagePacket()
            {
                Message = packet.Message,
            };

            _serverConnectors.Send(x => x.Data?.ToString() == packet.Username, module, command, msgPacket);
        }
    }
}
