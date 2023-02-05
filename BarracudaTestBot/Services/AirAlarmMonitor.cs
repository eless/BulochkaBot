namespace BarracudaTestBot.Services
{
    public class AirAlarmMonitor : BackgroundService
    {
        private AirAlarmChecker _checker;
        private AirAlarmGenericNotifier _notifier;
        private readonly TimeSpan _alarmCheckPeriod = TimeSpan.FromSeconds(5);

        public AirAlarmMonitor(AirAlarmChecker checker, AirAlarmGenericNotifier notifier)
        {
            _checker = checker;
            _notifier = notifier;
        }

        protected override async Task ExecuteAsync(CancellationToken cts)
        {
            try
            {
                using var timer = new PeriodicTimer(_alarmCheckPeriod);
                while (!cts.IsCancellationRequested)
                {
                    await timer.WaitForNextTickAsync();
                    var result = _checker.Check().Result;
                    _notifier.notify(result);
                    // TODO: maybe add report at midnight about all alerts this day, and wish a good night.
                }
            }
            catch(Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"The following exception have occurred: {ex.GetType().ToString()}");
            }
        }
    }
}
