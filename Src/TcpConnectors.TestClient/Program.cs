using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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




            _menusHandler.TopLevelMenu();


            //while (true)
            //{
            //    ConsoleUI.ShowMenu("test", Action1, Action2);
            //    var line = Console.ReadLine();


            //    if (line == "Q")
            //    {
            //        break;
            //    }
            //    else if (line == "CG")
            //    {
            //        var res = _clientConnector.SendRequest(CreateGroupRequestPacket.MODULE, CreateGroupRequestPacket.COMMAND, new CreateGroupRequestPacket { GroupName = "MyGroup" });
            //        Console.WriteLine($"Response: { JsonConvert.SerializeObject(res)}");
            //    }
            //    else if (line == "r")
            //    {
            //        try
            //        {
            //            var res = _clientConnector.SendRequestAsync(1, 1, "reqRes Async").Result;
            //            Console.WriteLine($"Response: { res}");
            //        }
            //        catch (Exception ex)
            //        {
            //            Console.WriteLine("Exception in SendRequestAsync");
            //        }
            //    }
            //    else if (line == "R")
            //    {
            //        try
            //        {
            //            var res = _clientConnector.SendRequest(1, 1, "reqRes blocking");
            //            Console.WriteLine($"Response: { res}");
            //        }
            //        catch (Exception ex)
            //        {
            //            Console.WriteLine("Exception in SendRequest");
            //        }
            //    }
            //    else
            //    {
            //        var msgPacket = new SendGroupMessagePacket()
            //        {
            //            Message = line
            //        };
            //        _clientConnector.Send(3, 1, msgPacket);
            //    }

            //}

            //Console.WriteLine("Press Enter to continue...");
            //Console.ReadLine();
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

        private static void Login()
        {
            var loginRes = _clientConnector.SendRequest(LoginRequestPacket.MODULE, LoginRequestPacket.MODULE, new LoginRequestPacket() { Username = "u" });
            Console.WriteLine($"loginRes: {JsonConvert.SerializeObject(loginRes)}");
        }

        private static void ClientConnector_OnPacket(int module, int command, object packet)
        {
            Console.WriteLine($"ClientConnector_OnPacket!!!! module:{module} command:{command} packet:{JsonConvert.SerializeObject(packet)}");
        }


    }
}
