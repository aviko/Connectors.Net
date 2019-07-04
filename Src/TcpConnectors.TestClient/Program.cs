using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using TcpConnectors.TestCommon;

namespace TcpConnectors.TestClient
{
    // todo:
    // split code to UI + packets handlers + model
    // ext method for send by type
    // timeout on send request

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("TcpConnectors.TestClient");


            var clientConnector = new ClientConnector(new ClientConnectorSettings()
            {
                PacketsMap = PacketsUtils.GetServer2ClientMapping(),
                ServerAddressList = new List<Tuple<string, int>>() { new Tuple<string, int>("127.0.0.1", 1111) }
            });

            clientConnector.OnPacket += ClientConnector_OnPacket;
            clientConnector.OnConnect += ClientConnector_OnConnect;
            clientConnector.OnDisconnect += ClientConnector_OnDisconnect;
            clientConnector.OnException += ClientConnector_OnException;

            clientConnector.Connect();


            var loginRes = clientConnector.SendRequest(LoginRequestPacket.MODULE, LoginRequestPacket.MODULE, new LoginRequestPacket() { Username = "u" });

            Console.WriteLine($"loginRes: {JsonConvert.SerializeObject(loginRes)}");




            while (true)
            {
                var line = Console.ReadLine();
                if (line == "Q")
                {
                    break;
                }
                else if (line == "CG")
                {
                    var res = clientConnector.SendRequest(CreateGroupRequestPacket.MODULE, CreateGroupRequestPacket.COMMAND, new CreateGroupRequestPacket {GroupName = "MyGroup" });
                    Console.WriteLine($"Response: { JsonConvert.SerializeObject(res)}");
                }
                else if (line == "r")
                {
                    var res = clientConnector.SendRequestAsync(1, 1, "reqRes").Result;
                    Console.WriteLine($"Response: { res}");
                }
                else
                {
                    var msgPacket = new SendGroupMessagePacket()
                    {
                        Message = line
                    };
                    clientConnector.Send(3, 1, msgPacket);
                }

            }

            Console.WriteLine("Press Enter to continue...");
            Console.ReadLine();
        }

        private static void ClientConnector_OnException(Exception exp)
        {
            Console.WriteLine($"ClientConnector_OnException {exp}");
        }

        private static void ClientConnector_OnDisconnect()
        {
            Console.WriteLine($"ClientConnector_OnDisconnect");
        }

        private static void ClientConnector_OnConnect()
        {
            Console.WriteLine($"ClientConnector_OnConnect");
        }

        private static void ClientConnector_OnPacket(int module, int command, object packet)
        {
            Console.WriteLine($"ClientConnector_OnPacket!!!! module:{module} command:{command} packet:{JsonConvert.SerializeObject(packet)}");
        }


    }
}
