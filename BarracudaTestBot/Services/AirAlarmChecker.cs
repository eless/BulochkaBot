
using Microsoft.ApplicationInsights;

namespace BarracudaTestBot.Services
{
    public class AirAlarmChecker
    {
        private TelemetryClient _telemetry;
        private HttpClient _httpClient;
        public AirAlarmChecker(TelemetryClient telemetry, HttpClient httpClient)
        {
            _telemetry = telemetry;
            _httpClient = httpClient;
        }
        private bool _KyivAlertActive = false;
        private readonly string _airAlertTelegramChannelLink = "https://t.me/s/air_alert_ua";

        public enum AlertStatus
        {
            NotChanged,
            Active,
            AllClear,
            FatalError
        };

        public async Task<AlertStatus> Check()
        {
            var status = AlertStatus.NotChanged;
            try
            {
                var alert = _KyivAlertActive;
                var content = await _httpClient.GetStringAsync(_airAlertTelegramChannelLink);

                using StringReader reader = new StringReader(content);
                while(true)
                {
                    var line = reader.ReadLine();
                    if (line == null)
                    {
                        break;
                    }
                    if (line.Contains("Повітряна тривога в м. Київ"))
                    {
                        alert = true;
                    }
                    if (line.Contains("Відбій тривоги в м. Київ"))
                    {
                        alert = false;
                    }
                }

                if (_KyivAlertActive != alert)
                {
                    _KyivAlertActive = alert;
                    status = _KyivAlertActive ? AlertStatus.Active : AlertStatus.AllClear;
                }
            }
            catch (Exception ex)
            {
                _telemetry.TrackTrace($"ALERT POLL FAILED: {ex.Message}");
                _telemetry.TrackException(ex);
                status = AlertStatus.FatalError;
            }
            return status;
        }
    }
}
