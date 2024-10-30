
namespace BarracudaTestBot.Services
{
    public class RussianLossesDailyReport : BackgroundService
    {
        private RussianLossesService _russianLossesService;
        private RussianLossesSender _russianLossesSender;

        public RussianLossesDailyReport(RussianLossesService russianLossesService, RussianLossesSender russianLossesSender)
        {
            _russianLossesService = russianLossesService;
            _russianLossesSender = russianLossesSender;
        }

        protected override async Task ExecuteAsync(CancellationToken cts)
        {
            DateTime kyivReportTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 12, 0, 0);
            TimeZoneInfo kyivTimeZone = TimeZoneInfo.FindSystemTimeZoneById("E. Europe Standard Time");
            DateTime _reportTime = TimeZoneInfo.ConvertTimeToUtc(kyivReportTime, kyivTimeZone);

            DateTime now = DateTime.UtcNow;
            if (now > _reportTime)
            {
                _reportTime = _reportTime.AddDays(1);
            }
            var delay = _reportTime - now;
            await Task.Delay(delay, cts);
            using PeriodicTimer timer = new PeriodicTimer(TimeSpan.FromDays(1));

            while (!cts.IsCancellationRequested)
            {
                System.Diagnostics.Trace.WriteLine("RussianLossesDailyReport");
                var losses = await _russianLossesService.GetData(cts);
                await _russianLossesSender.Send(losses);
                await timer.WaitForNextTickAsync();
            }
        }
    }
}
