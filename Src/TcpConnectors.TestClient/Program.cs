using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TcpConnectors.TestCommon;

namespace TcpConnectors.TestClient
{
    // todo:
    // split code to UI + packets handlers + model
    // ext method for send by type
    // timeout on send request

    class Program
    {

        private static MenusHandler _menusHandler = new MenusHandler();

        internal static ClientConnector _clientConnector = null;
        internal static string _username;

        static void Main(string[] args)
        {
            Console.WriteLine("TcpConnectors.TestClient");


            _clientConnector = new ClientConnector(new ClientConnectorSettings()
            {
                PacketsMap = PacketsUtils.GetServer2ClientMapping(),
                ServerAddressList = new List<Tuple<string, int>>() { new Tuple<string, int>("127.0.0.1", 1111) }
            });

            _clientConnector.OnPacket += ClientConnector_OnPacket;
            _clientConnector.OnConnect += ClientConnector_OnConnect;
            _clientConnector.OnDisconnect += ClientConnector_OnDisconnect;
            _clientConnector.OnException += ClientConnector_OnException;
            _clientConnector.OnDebugLog += ClientConnector_OnDebugLog; ;

            _clientConnector.Connect();

            Thread.Sleep(-1);

        }

        private static void ClientConnector_OnDebugLog(DebugLogType logType, string info)
        {
            Console.WriteLine($"ClientConnector_OnDebugLog logType:{logType} info:{info}");
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
            new Task(Login).Start();
        }

        private static async void Login()
        {
            while (true)
            {
                Console.WriteLine($"Enter Username");
                _username = Console.ReadLine();
                var loginRes = await _clientConnector.SendRequestAsync(LoginRequestPacket.MODULE, LoginRequestPacket.MODULE, new LoginRequestPacket() { Username = _username }) as LoginResponsePacket;
                Console.WriteLine($"loginRes: {JsonConvert.SerializeObject(loginRes)}");

                if (loginRes.RetCode == true)
                {
                    if (_menusHandler._isStarted == false)
                    {
                        new Task(() => _menusHandler.TopLevelMenu()).Start();
                    }
                    return;
                }
            }
        }

        private static void ClientConnector_OnPacket(int module, int command, object packet)
        {
            Console.WriteLine($"ClientConnector_OnPacket!!!! module:{module} command:{command} packet:{JsonConvert.SerializeObject(packet)}");

            //Console.WriteLine($"start sleep ");
            //Thread.Sleep(40 * 1000);
            //Console.WriteLine($"end sleep ");

        }


    }
}
