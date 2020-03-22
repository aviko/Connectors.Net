using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using StressSimulatorCommon;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using TcpConnectors;

namespace StressSimulatorClient
{

    public class AppSettingsClient
    {
        public int NumOfClients { get; set; }
        public int ReportRequestSecondsInterval { get; set; }
        public string ServerAddress { get; set; }
        public int ReconnectMinutsInterval { get; set; }
    }

    class Program
    {
        public static AppSettingsClient AppSettingsClient { get; set; }
        public static ConcurrentDictionary<int, ClientBot> BotsMap = new ConcurrentDictionary<int, ClientBot>();

        static void Main(string[] args)
        {
            Console.WriteLine("TestSimpleEchoClient");


            IConfiguration config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true, true)
                .Build();

            AppSettingsClient = config.GetSection("AppSettingsClient").Get<AppSettingsClient>();

            Console.WriteLine($"AppSettingsClient: {JsonConvert.SerializeObject(AppSettingsClient, Formatting.Indented)}");


            for (int i = 0; i < AppSettingsClient.NumOfClients; i++)
            {
                var clientBot = new ClientBot();
                BotsMap[i] = clientBot;
            }



            //while (true)
            //{
            //    try
            //    {
            //        Console.WriteLine("Enter Input");
            //        var inputLine = Console.ReadLine();

            //        if (inputLine == "exp")
            //        {
            //            _clientConnector.SendRequest(1, 1, inputLine);
            //        }
            //        else
            //        {
            //            _clientConnector.Send(1, 1, inputLine);
            //        }


            //    }
            //    catch (Exception ex)
            //    {
            //        Console.WriteLine("Exception:" + ex.ToString());
            //    }
            //}

            Console.ReadLine();

        }

    }
}
