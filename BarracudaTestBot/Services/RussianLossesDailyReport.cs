using BarracudaTestBot.Checkers;
using System.Threading;
using Telegram.Bot;
using static System.Net.Mime.MediaTypeNames;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using System.Diagnostics.Eventing.Reader;
using System.Timers;

namespace BarracudaTestBot.Services
{
    public class RussianLossesDailyReport : BackgroundService
    {
        private RussianLossesService _russianLossesService;
        private RussianLossesSender _russianLossesSender;
        private DateTime _reportTime = new DateTime(year: DateTime.Now.Year, month: DateTime.Now.Month, day: DateTime.Now.Day, hour: 12, minute: 00, second: 00);

        public RussianLossesDailyReport(RussianLossesService russianLossesService, RussianLossesSender russianLossesSender)
        {
            _russianLossesService = russianLossesService;
            _russianLossesSender = russianLossesSender;
        }

        protected override async Task ExecuteAsync(CancellationToken cts)
        {
            DateTime now = DateTime.Now;
            if (now > _reportTime)
            {
                _reportTime = _reportTime.AddDays(1);
            }
            var delay = _reportTime - now;
            await Task.Delay(delay);
            using PeriodicTimer timer = new PeriodicTimer(TimeSpan.FromDays(1));

            while (!cts.IsCancellationRequested)
            {
                System.Diagnostics.Trace.WriteLine("RussianLossesDailyReport");
                var losses = _russianLossesService.GetData().Result;
                _russianLossesSender.Send(losses);
                await timer.WaitForNextTickAsync();
            }
        }
    }
}
