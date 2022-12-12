namespace BarracudaTestBot.Services
{
    public class PingService : BackgroundService
    {
        private int _pingPeriodMin = 10;
        private string urlToPing = "https://barracudatestbot.azurewebsites.net";
        private int PingPeriod => 1000 * 60 * _pingPeriodMin;

        protected override async Task ExecuteAsync(CancellationToken cts)
        {
            using var client = new HttpClient();
            while (!cts.IsCancellationRequested)
            {
                await Task.Delay(PingPeriod, cts);
                try
                {
                    var content = await client.GetStringAsync(urlToPing);
                }
                catch (HttpRequestException hre)
                {
                    System.Diagnostics.Trace.WriteLine(hre);
                }
            }
        }
    }
}
