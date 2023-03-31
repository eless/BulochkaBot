using BarracudaTestBot.Checkers;
using Telegram.Bot;
using System;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Channel;

namespace BarracudaTestBot.Services
{
    public class AirAlarmChecker
    {
        private TelemetryClient _telemetry;
        public AirAlarmChecker(TelemetryClient telemetry)
        {
            _telemetry = telemetry;
        }
        private static bool _KyivAlertActive = false;

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
                using var client = new HttpClient();
                var content = await client.GetStringAsync("https://t.me/s/air_alert_ua");

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
                _telemetry.TrackTrace("ALERT POLL FAILED");
                _telemetry.TrackException(ex);
                status = AlertStatus.FatalError;
            }
            return status;
        }
    }
}
