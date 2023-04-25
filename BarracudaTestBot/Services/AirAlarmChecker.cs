
using Microsoft.ApplicationInsights;
using HtmlAgilityPack;

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
                var lastMentionElement = doc.DocumentNode.DescendantsAndSelf()
                    .Where(e => e.NodeType == HtmlNodeType.Text && e.InnerText.Contains("м. Київ"))
                    .LastOrDefault();

                var alert = _KyivAlertActive;
                if (lastMentionElement != null)
                {
                    // Traverse up the DOM tree to find the parent div element containing the message
                    var messageDiv = lastMentionElement.Ancestors("div").FirstOrDefault();
                    if (messageDiv != null)
                    {
                        if (messageDiv.InnerText.Contains("Відбій тривоги"))
                        {
                            alert = false;
                        }
                        else if (messageDiv.InnerText.Contains("Повітряна тривога"))
                        {
                            alert = true;
                        }
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
