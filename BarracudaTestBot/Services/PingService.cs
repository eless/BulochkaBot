using Microsoft.ApplicationInsights;

namespace BarracudaTestBot.Services
{
    public class PingService : BackgroundService
    {
        private TelemetryClient _telemetry;
        private HttpClient _httpClient;
        public PingService(TelemetryClient telemetry, HttpClient httpClient)
        {
            _telemetry = telemetry;
            _httpClient = httpClient;
        }
        private int _pingPeriodMin = 10;
        private string urlToPing = "https://barracudatestbot.azurewebsites.net";
        private TimeSpan PingPeriod => TimeSpan.FromMinutes(_pingPeriodMin);

        protected override async Task ExecuteAsync(CancellationToken cts)
        {
            while (!cts.IsCancellationRequested)
            {
                bool success = true;
                try
                {
                    var content = await _httpClient.GetStringAsync(urlToPing);
                }
                catch (TaskCanceledException)
                {
                    _telemetry.TrackTrace("PING TASK CANCELLED!");
                    break;
                }
                catch (Exception ex)
                {
                    _telemetry.TrackTrace($"PING FAILED: {ex.Message}");
                    _telemetry.TrackException(ex);
                    success = false;
                }
                await Task.Delay(success ? PingPeriod : TimeSpan.FromSeconds(1), cts);
            }
        }
    }
}
