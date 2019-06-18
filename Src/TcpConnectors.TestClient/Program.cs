using System;
using System.Collections.Generic;

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

            clientConnector.OnPacket += ClientConnector_OnPacket;


            while (true)
            {
                var line = Console.ReadLine();
                clientConnector.Send(1, 1, line);

                if (line == "Q")
                {
                    break;
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
                { new Tuple<int,int>(1,1) , typeof(string) }
            };
        }

    }
}
