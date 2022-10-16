using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using Telegram.Bot;

namespace BarracudaTestBot
{
    public static class Utilities
    {
        public static int GetRandomNumber(int from, int to) => new Random().Next(from, to);

    }

    public class PingService : BackgroundService
    {
        private int pingPeriod = 1000 * 60;
        private string urlToPing = "https://barracudatestbot.azurewebsites.net";

        protected override async Task ExecuteAsync(CancellationToken cts)
        {
            while(!cts.IsCancellationRequested)
            {
                await Task.Delay(pingPeriod, cts);
                using var client = new HttpClient();
                try {
                    var content = await client.GetStringAsync(urlToPing);
                } catch (HttpRequestException hre) {
                    Console.WriteLine(hre);
                }
            }
        }
    }
}
