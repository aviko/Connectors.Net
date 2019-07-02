using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using TcpConnectors.TestCommon;

namespace TcpConnectors.TestClient
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("TcpConnectors.TestClient");


            var clientConnector = new ClientConnector(new ClientConnectorSettings()
            {
                TypesMap = PacketsUtils.GetServer2ClientMapping(),
                ServerAddressList = new List<Tuple<string, int>>() { new Tuple<string, int>("127.0.0.1", 1111) }
            });

            clientConnector.Connect();
            clientConnector.OnPacket += ClientConnector_OnPacket;
            clientConnector.OnConnect += ClientConnector_OnConnect;
            clientConnector.OnDisconnect += ClientConnector_OnDisconnect;
            clientConnector.OnException += ClientConnector_OnException;

            var loginRes = clientConnector.SendRequest(1, 1, new LoginRequestPacket() { Username = "u" });

            Console.WriteLine($"loginRes: {JsonConvert.SerializeObject(loginRes)}");




            while (true)
            {
                var line = Console.ReadLine();
                if (line == "Q")
                {
                    break;
                }
                else if (line == "R")
                {
                    var res = clientConnector.SendRequest(1, 1, "ReqRes");
                    Console.WriteLine($"Response: { res}");
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

        //static Dictionary<Tuple<int, int>, Type> GetTypeMap()
        //{
        //    return new Dictionary<Tuple<int, int>, Type>()
        //    {
        //        { new Tuple<int,int>(1,1) , typeof(string) },
        //        { new Tuple<int,int>(2,1) , typeof(string) },
        //        { new Tuple<int,int>(3,1) , typeof(LoginRequestPacket) }


        //    };
        //}

    }
}
