namespace BarracudaTestBot.Services
{
    public class PingService : BackgroundService
    {
        private int pingPeriod = 1000 * 60 * 10;
        private string urlToPing = "https://barracudatestbot.azurewebsites.net";
        private BotService _botService;

        public PingService(BotService botService) => _botService = botService;

        protected override async Task ExecuteAsync(CancellationToken cts)
        {
            var start = new TimeSpan(7, 0, 0);
            var end = new TimeSpan(7, pingPeriod + 2, 0);
            using var client = new HttpClient();
            while (!cts.IsCancellationRequested)
            {
                await Task.Delay(pingPeriod, cts);
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
                    Console.WriteLine(hre);
                }
            }
        }
    }
}
