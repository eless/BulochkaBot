using BarracudaTestBot.Checkers;
using Telegram.Bot;
using System;

namespace BarracudaTestBot.Services
{
    public class AirAlarmChecker
    {
        private static bool KyivAlertActive = false;

        public enum AlertStatus : ushort
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
                var alert = KyivAlertActive;
                using var client = new HttpClient();
                var content = await client.GetStringAsync("https://t.me/s/air_alert_ua");
                
                StringReader reader = new StringReader(content);
                while(true)
                {
                    string line = reader.ReadLine();
                    if (string.IsNullOrEmpty(line))
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

                if (KyivAlertActive != alert)
                {
                    KyivAlertActive = alert;
                    status = KyivAlertActive ? AlertStatus.Active : AlertStatus.AllClear;
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
