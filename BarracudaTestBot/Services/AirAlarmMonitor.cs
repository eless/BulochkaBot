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
        private bool _success = true;

        public AirAlarmMonitor(AirAlarmChecker checker, AirAlarmGenericNotifier notifier, TelemetryClient telemetry)
        {
            _checker = checker;
            _notifier = notifier;
            _telemetry = telemetry;
        }

        private void HandleCheckFailure(Exception ex)
        {
            if (ex is TaskCanceledException)
            {
                _telemetry.TrackTrace("ALARM MONITOR CANCELLED!");
            }
            else
            {
                _telemetry.TrackTrace($"ALARM MONITOR FAILED: {ex.Message}");
                _telemetry.TrackException(ex);
            }
            _success = false;
        }

        protected override async Task ExecuteAsync(CancellationToken cts)
        {
            while (!cts.IsCancellationRequested)
            {
                try
                {
                    var result = await _checker.Check();
                    _notifier.notify(result);
                    _success = true;
                    // TODO: maybe add report at midnight about all alerts this day, and wish a good night.
                }
                catch (Exception ex)
                {
                    HandleCheckFailure(ex);
                }
                await Task.Delay(_success ? _alarmCheckPeriod : TimeSpan.FromSeconds(1), cts);
            }
        }
    }
}
