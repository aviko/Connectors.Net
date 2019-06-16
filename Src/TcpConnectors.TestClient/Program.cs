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

            Console.WriteLine("Press Enter to continue...");
            Console.ReadLine();
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
