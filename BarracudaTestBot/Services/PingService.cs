namespace BarracudaTestBot.Services
{
    public class PingService : BackgroundService
    {
        private int _pingPeriodMin = 10;
        private int _pingHourUtc = 8;

        private string urlToPing = "https://barracudatestbot.azurewebsites.net";
        private BotService _botService;

        public PingService(BotService botService) => _botService = botService;

        private int PingPeriod => 1000 * 60 * _pingPeriodMin;

        protected override async Task ExecuteAsync(CancellationToken cts)
        {
            var start = new TimeSpan(_pingHourUtc, 0, 0);
            var end = new TimeSpan(_pingHourUtc, _pingPeriodMin + 2, 0);
            using var client = new HttpClient();
            while (!cts.IsCancellationRequested)
            {
                await Task.Delay(PingPeriod, cts);
                try
                {
                    var content = await client.GetStringAsync(urlToPing);

                    var now = DateTime.UtcNow.TimeOfDay;
                    if (now > start && now < end)
                    {
                        await _botService.SendLosses();
                    }
                }
                catch (HttpRequestException hre)
                {
                    System.Diagnostics.Trace.WriteLine(hre);
                }
            }
        }
    }
}
