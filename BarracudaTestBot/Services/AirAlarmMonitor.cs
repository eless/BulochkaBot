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
        private readonly TimeSpan _alarmCheckPeriod = TimeSpan.FromSeconds(5);

        public AirAlarmMonitor(AirAlarmChecker checker, AirAlarmGenericNotifier notifier, TelemetryClient telemetry)
        {
            _checker = checker;
            _notifier = notifier;
            _telemetry = telemetry;
        }

        protected override async Task ExecuteAsync(CancellationToken cts)
        {
            try
            {
                using var timer = new PeriodicTimer(_alarmCheckPeriod);
                while (!cts.IsCancellationRequested)
                {
                    await timer.WaitForNextTickAsync();
                    _telemetry.TrackEvent("ALERT POLLING");
                    var result = _checker.Check().Result;
                    _notifier.notify(result);
                    // TODO: maybe add report at midnight about all alerts this day, and wish a good night.
                }
            }
            catch (Exception ex)
            {
                _telemetry.TrackException(ex);
            }
        }
    }
}
