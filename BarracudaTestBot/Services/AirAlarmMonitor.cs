namespace BarracudaTestBot.Services
{
    public class AirAlarmMonitor
    {
        AirAlarmChecker _checker;
        AirAlarmGenericNotifier _notifier;

        AirAlarmMonitor(AirAlarmChecker checker, AirAlarmGenericNotifier notifier)
        {
            _checker = checker;
            _notifier = notifier;
        }

        public async void Start()
        {
            using var timer = new PeriodicTimer(TimeSpan.FromSeconds(5));

            while (await timer.WaitForNextTickAsync())
            {
                var result = _checker.Check().Result;
                _notifier.notify(result);
                // TODO: maybe add report at midnight about all alerts this day, and wish a good night.
            }
        }
    }
}
