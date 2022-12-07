namespace BarracudaTestBot.Services
{
    public class AirAlarmGenericNotifier
    {
        private AirAlarmAlertNotifier _alertNotifier;
        private AirAlarmAllClearNotifier _allClearNotifier;

        public AirAlarmGenericNotifier(AirAlarmAlertNotifier alertNotifier, AirAlarmAllClearNotifier allClearNotifier)
        {
            _alertNotifier = alertNotifier;
            _allClearNotifier = allClearNotifier;
        }

        public void notify(AirAlarmChecker.AlertStatus status)
        {
            switch (status)
            {
                case AirAlarmChecker.AlertStatus.Active:
                    _alertNotifier.Send();
                    break;

                case AirAlarmChecker.AlertStatus.AllClear:
                    _allClearNotifier.Send();
                    break;

                case AirAlarmChecker.AlertStatus.FatalError:
                    // TODO: add some trace log, or throw a text error in chat
                    break;

                default:
                    break;
            }
        }
    }
}
