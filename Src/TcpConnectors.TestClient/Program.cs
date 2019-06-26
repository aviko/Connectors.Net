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

            var clientConnector = new ClientConnector(GetTypeMap());

            clientConnector.Connect("127.0.0.1", 1111);

            clientConnector.Send(1, 1, "hello !");
            clientConnector.Send(3, 1, new LoginPayload() { Username="u", EncPassword="p" });

            clientConnector.OnPacket += ClientConnector_OnPacket;


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
                    clientConnector.Send(1, 1, line);
                }

            }

            Console.WriteLine("Press Enter to continue...");
            Console.ReadLine();
        }

        private static void ClientConnector_OnPacket(int arg1, int arg2, object arg3)
        {
            Console.WriteLine($"ClientConnector_OnPacket!!!! { arg3}");
        }

        static Dictionary<Tuple<int, int>, Type> GetTypeMap()
        {
            return new Dictionary<Tuple<int, int>, Type>()
            {
                { new Tuple<int,int>(1,1) , typeof(string) },
                { new Tuple<int,int>(2,1) , typeof(string) },
                { new Tuple<int,int>(3,1) , typeof(LoginPayload) }


            };
        }

    }
}
