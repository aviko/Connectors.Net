using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TcpConnectors;

namespace TestSimpleEchoClient
{
    class Program
    {
        internal static ClientConnector _clientConnector = null;
        static void Main(string[] args)
        {
            Console.WriteLine("TestSimpleEchoClient");


            _clientConnector = new ClientConnector(new ClientConnectorSettings()
            {
                PacketsMap = new Dictionary<Tuple<int, int>, Type>() { { new Tuple<int, int>(1, 1), typeof(string) }, },
                ServerAddressList = new List<Tuple<string, int>>() { new Tuple<string, int>("127.0.0.1", 1112) }
            });

            _clientConnector.OnPacket += ClientConnector_OnPacket;
            _clientConnector.OnConnect += ClientConnector_OnConnect;
            _clientConnector.OnDisconnect += ClientConnector_OnDisconnect;
            _clientConnector.OnException += ClientConnector_OnException;
            _clientConnector.OnDebugLog += ClientConnector_OnDebugLog;

            _clientConnector.Connect();


            try { _clientConnector.Send(1, 1, "start"); } catch (Exception ex) { Console.WriteLine("Exception on first packet:" + ex.ToString()); }

            while (true)
            {
                try
                {
                    Console.WriteLine("Enter Input");
                    var inputLine = Console.ReadLine();
                    _clientConnector.Send(1, 1, inputLine);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Exception:" + ex.ToString());
                }
            }

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
            Console.WriteLine($"ClientConnector_OnPacket module:{module} command:{command} packet:{JsonConvert.SerializeObject(packet)}");
        }

        private static void ClientConnector_OnDebugLog(DebugLogType logType, string info)
        {
            Console.WriteLine($"ClientConnector_OnDebugLog logType:{logType} info:{info}");
        }

        private static void ClientConnector_OnException(Exception exp)
        {
            Console.WriteLine($"ClientConnector_OnException {exp}");
        }
    }
}
