using Microsoft.ApplicationInsights;
using HtmlAgilityPack;
using System.Text.RegularExpressions;

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
                var content = await _httpClient.GetStringAsync(_airAlertTelegramChannelLink);
                var doc = new HtmlDocument();
                doc.LoadHtml(content);

                // Find the last mention of "м. Київ" on the page
                var lastMentionElement = doc.DocumentNode.Descendants("div")
                    .Where(e => e.HasClass("tgme_widget_message_text") &&
                                Regex.IsMatch(e.InnerText, @"м\.\s*Київ\.?", RegexOptions.IgnoreCase))
                    .LastOrDefault();

                var alert = _KyivAlertActive;
                if (lastMentionElement != null)
                {
                    if (Regex.IsMatch(lastMentionElement.InnerText, "Відбій тривоги", RegexOptions.IgnoreCase))
                    {
                        alert = false;
                    }
                    else if (Regex.IsMatch(lastMentionElement.InnerText, "Повітряна тривога", RegexOptions.IgnoreCase))
                    {
                        alert = true;
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
