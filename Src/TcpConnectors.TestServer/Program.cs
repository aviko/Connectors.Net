using System;
using System.Collections.Generic;
using TcpConnectors.TestCommon;

namespace TcpConnectors.TestServer
{
    class Program
    {

        internal static ConnectorsHandler ConnectorsHandler { get; set; }
        internal static ChatServerModel ChatServerModel { get; set; }

        static void Main(string[] args)
        {
            Console.WriteLine("TcpConnectors.TestServer");

            ChatServerModel = new ChatServerModel();
            ConnectorsHandler = new ConnectorsHandler();



            Console.WriteLine("Press Enter to continue...");
            Console.ReadLine();

        }

    }
}
