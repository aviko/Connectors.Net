using Newtonsoft.Json;
using StressSimulatorCommon;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Timers;
using TcpConnectors;

namespace StressSimulatorClient
{
    public class ClientBot
    {
        private static Dictionary<Tuple<int, int>, Type> _packetsMap;
        private ClientConnector _clientConnector = null;
        private System.Timers.Timer _reportRequestTimer = null;
        private static int _randSeed = 100;
        private Random _rnd = new Random(_randSeed++);

        static ClientBot()
        {
            _packetsMap = SendPacketsUtils.GetServer2ClientMapping(Assembly.GetAssembly(typeof(LoginRequestPacket)));
        }

        public ClientBot()
        {
            _clientConnector = new ClientConnector(new ClientConnectorSettings()
            {
                PacketsMap = new Dictionary<Tuple<int, int>, Type>(_packetsMap),
                ServerAddressList = new List<Tuple<string, int>>() { new Tuple<string, int>(Program.AppSettingsClient.ServerAddress, 1112) }
            });

            _clientConnector.OnPacket += ClientConnector_OnPacket;
            _clientConnector.OnConnect += ClientConnector_OnConnect;
            _clientConnector.OnDisconnect += ClientConnector_OnDisconnect;
            _clientConnector.OnException += ClientConnector_OnException;
            _clientConnector.OnDebugLog += ClientConnector_OnDebugLog;

            _clientConnector.Connect();

            object loginResponse = null;
            try { loginResponse = _clientConnector.SendRequest(new LoginRequestPacket { Username = "test" }); } catch (Exception ex) { Console.WriteLine("Exception on first packet:" + ex.ToString()); }
            Console.WriteLine($"Response loginResponse:{JsonConvert.SerializeObject(loginResponse)}");

            _reportRequestTimer = new System.Timers.Timer(1000);
            _reportRequestTimer.Elapsed += ReportRequest_Elapsed; ;
            _reportRequestTimer.Start();

        }

        private void ReportRequest_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (_rnd.Next(Program.AppSettingsClient.ReportRequestSecondsInterval) == 0)
            {
                var res = _clientConnector.SendRequest(new ReportRequestPacket() { ReportName = "X" }) as ReportResponsePacket;
                Console.WriteLine(res.Records.Count);
            }
        }

        private void ClientConnector_OnDisconnect()
        {
            //Console.WriteLine($"ClientConnector_OnDisconnect");
        }

        private void ClientConnector_OnConnect()
        {
            //Console.WriteLine($"ClientConnector_OnConnect");
        }

        private void ClientConnector_OnPacket(int module, int command, object packet)
        {
            //Console.Write("q");
            //Console.WriteLine($"ClientConnector_OnPacket module:{module} command:{command} packet:{JsonConvert.SerializeObject(packet)}");
        }

        private void ClientConnector_OnDebugLog(DebugLogType logType, string info)
        {
            //Console.WriteLine($"ClientConnector_OnDebugLog logType:{logType} info:{info}");
        }

        private void ClientConnector_OnException(Exception exp)
        {
            //Console.WriteLine($"ClientConnector_OnException {exp}");
        }


    }
}
