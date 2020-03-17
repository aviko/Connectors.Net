using StressSimulatorCommon;
using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;

namespace StressSimulatorServer
{
    class QuotesSender
    {
        private System.Timers.Timer _sendTimer = null;

        public QuotesSender()
        {
            _sendTimer = new System.Timers.Timer(Program.AppSettingsServer.SendQuotesIntevalMillis);
            _sendTimer.Elapsed += SendTimer_Elapsed; ;
            _sendTimer.Start();
        }

        private void SendTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Program.ConnectorsHandler.SendToAll(new UpdateQuotePackets()
            {
                Symbol = "EURUSD",
                Bid = 1.1m,
                Ask = 1.2m,
            });
        }
    }
}
