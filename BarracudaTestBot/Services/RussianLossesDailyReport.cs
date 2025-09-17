
using Microsoft.ApplicationInsights;

namespace BarracudaTestBot.Services
{
    public class RussianLossesDailyReport(
        RussianLossesService russianLossesService,
        RussianLossesSender russianLossesSender,
        TelemetryClient telemetry) : BackgroundService
    {

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
                try
                {
                    System.Diagnostics.Trace.WriteLine("RussianLossesDailyReport");
                    var losses = await russianLossesService.GetData(cts);
                    await russianLossesSender.Send(losses);
                }
                catch (TaskCanceledException)
                {
                    telemetry?.TrackTrace("DAILY REPORT TASK CANCELLED!");
                    break;
                }
                catch (Exception ex)
                {
                    telemetry?.TrackTrace($"DAILY REPORT FAILED: {ex.Message}");
                    telemetry?.TrackException(ex);
                }
                await timer.WaitForNextTickAsync(cts);
            }
        }
    }
}
