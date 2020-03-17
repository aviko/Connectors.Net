using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using StressSimulatorCommon;
using System;
using System.Collections.Generic;
using System.Reflection;
using TcpConnectors;

namespace StressSimulatorServer
{

    public class AppSettingsServer
    {
        public int SendQuotesIntevalMillis { get; set; }
    }

    class Program
    {
        internal static AppSettingsServer AppSettingsServer { get; set; }
        internal static QuotesSender QuotesSender;
        internal static ConnectorsHandler ConnectorsHandler;

        static void Main(string[] args)
        {


            IConfiguration config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true, true)
                .Build();

            AppSettingsServer = config.GetSection("AppSettingsServer").Get<AppSettingsServer>();

            Console.WriteLine($"AppSettingsServer: {JsonConvert.SerializeObject(AppSettingsServer, Formatting.Indented)}");

            ConnectorsHandler = new ConnectorsHandler();
            QuotesSender = new QuotesSender();

            Console.ReadLine();

        }

    }
}
