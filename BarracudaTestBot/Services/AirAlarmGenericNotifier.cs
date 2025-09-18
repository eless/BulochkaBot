using Microsoft.ApplicationInsights;
namespace BarracudaTestBot.Services
{
    public class AirAlarmGenericNotifier
    {
        private AirAlarmAlertNotifier _alertNotifier;
        private AirAlarmAllClearNotifier _allClearNotifier;
        private TelemetryClient _telemetry;

        public AirAlarmGenericNotifier(AirAlarmAlertNotifier alertNotifier, AirAlarmAllClearNotifier allClearNotifier,
                                       TelemetryClient telemetry)
        {
            _alertNotifier = alertNotifier;
            _allClearNotifier = allClearNotifier;
            _telemetry = telemetry;
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
                    _telemetry.TrackTrace("AlertStatus.FatalError");
                    break;

                default:
                    break;
            }
        }
    }
}
