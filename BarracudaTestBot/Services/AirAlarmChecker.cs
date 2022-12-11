using BarracudaTestBot.Checkers;
using Telegram.Bot;
using System;

namespace BarracudaTestBot.Services
{
    public class AirAlarmChecker
    {
        public AirAlarmChecker()
        {

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
            catch (HttpRequestException hre)
            {
                System.Diagnostics.Trace.WriteLine(hre);
                status = AlertStatus.FatalError;
            }
            return status;
        }
    }
}
