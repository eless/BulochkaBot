using Microsoft.ApplicationInsights;

namespace BarracudaTestBot.Services
{
    public class PingService : BackgroundService
    {
        private TelemetryClient _telemetry;
        public PingService(TelemetryClient telemetry)
        {
            _telemetry = telemetry;
        }
        private int _pingPeriodMin = 10;
        private string urlToPing = "https://barracudatestbot.azurewebsites.net";
        private int PingPeriod => 1000 * 60 * _pingPeriodMin;

        protected override async Task ExecuteAsync(CancellationToken cts)
        {
            using var client = new HttpClient();
            while (!cts.IsCancellationRequested)
            {
                bool success = true;
                try
                {
                    var content = await client.GetStringAsync(urlToPing);
                }
                catch (TaskCanceledException)
                {
                    _telemetry.TrackTrace("PING TASK CANCELLED!");
                    break;
                }
                catch (Exception ex)
                {
                    _telemetry.TrackTrace("PING FAILED");
                    _telemetry.TrackException(ex);
                    success = false;
                }
                await Task.Delay(success ? PingPeriod : 1000, cts);
            }
        }
    }
}
