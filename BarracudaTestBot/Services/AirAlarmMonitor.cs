using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Channel;
using Telegram.Bot.Types;

namespace BarracudaTestBot.Services
{
    public class AirAlarmMonitor : BackgroundService
    {
        private AirAlarmChecker _checker;
        private AirAlarmGenericNotifier _notifier;
        TelemetryClient _telemetry;
        //private readonly TimeSpan _alarmCheckPeriod = TimeSpan.FromSeconds(5);

        public AirAlarmMonitor(AirAlarmChecker checker, AirAlarmGenericNotifier notifier, TelemetryClient telemetry)
        {
            _checker = checker;
            _notifier = notifier;
            _telemetry = telemetry;
        }

        protected override async Task ExecuteAsync(CancellationToken cts)
        {
            //using var timer = new PeriodicTimer(_alarmCheckPeriod);
            while (!cts.IsCancellationRequested)
            {
                bool success = true;
                try
                {
                    //await timer.WaitForNextTickAsync(); // TODO: temporary removed timer and add task delay
                    _telemetry.TrackEvent("ALERT POLLING");
                    var result = await _checker.Check();
                    _notifier.notify(result);
                    // TODO: maybe add report at midnight about all alerts this day, and wish a good night.
                }
                catch (TaskCanceledException)
                {
                    _telemetry.TrackTrace("ALARM MONITOR CANCELLED!");
                    break;
                }
                catch (Exception ex)
                {
                    _telemetry.TrackTrace("ALARM MONITOR FAILED");
                    _telemetry.TrackException(ex);
                    success = false;
                }
                await Task.Delay(success ? 5000 : 1000, cts);
            }
        }
    }
}
