namespace BarracudaTestBot.Services
{
    public class PingService : BackgroundService
    {
        private int pingPeriod = 1000 * 60 / 2;
        private string urlToPing = "https://barracudatestbot.azurewebsites.net";

        protected override async Task ExecuteAsync(CancellationToken cts)
        {
            while (!cts.IsCancellationRequested)
            {
                await Task.Delay(pingPeriod, cts);
                using var client = new HttpClient();
                try
                {
                    var content = await client.GetStringAsync(urlToPing);
                }
                catch (HttpRequestException hre)
                {
                    Console.WriteLine(hre);
                }
            }
        }
    }
}
