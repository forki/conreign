﻿using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using Conreign.Client.SignalR;
using Conreign.Core.AI;
using Conreign.Core.AI.LoadTest;
using Serilog;
using Serilog.Events;

namespace Conreign.LoadTest
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.LiterateConsole(restrictedToMinimumLevel: LogEventLevel.Information)
                //.WriteTo.File("log.txt", buffered: true, flushToDiskInterval: TimeSpan.FromSeconds(5))
                .WriteTo.Seq("http://localhost:5341")
                //.MinimumLevel.Verbose()
                .CreateLogger();
            Run().Wait();
        }

        private static async Task Run()
        {
            var testOptions = new LoadTestOptions
            {
                BotOptions = new LoadTestBotOptions
                {
                    RoomsCount = 50,
                    BotsPerRoomCount = 5
                }
            };
            ServicePointManager.DefaultConnectionLimit = testOptions.BotOptions.RoomsCount*
                                                         testOptions.BotOptions.BotsPerRoomCount*
                                                         2;
            var clientOptions = new SignalRClientOptions(testOptions.ConnectionUri);
            var client = new SignalRClient(clientOptions);
            var factory = new LoadTestBotFactory(testOptions.BotOptions);
            var farm = new BotFarm($"Farm-{Process.GetCurrentProcess().Id}", client, factory, new BotFarmOptions());
            await farm.Run();
        }
    }
}
