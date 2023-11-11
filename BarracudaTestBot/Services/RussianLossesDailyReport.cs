
namespace BarracudaTestBot.Services
{
    public class RussianLossesDailyReport : BackgroundService
    {
        private RussianLossesService _russianLossesService;
        private RussianLossesSender _russianLossesSender;
        private List<SubscriptionData> _subscribers;

        public RussianLossesDailyReport(RussianLossesService russianLossesService, RussianLossesSender russianLossesSender)
        {
            _russianLossesService = russianLossesService;
            _russianLossesSender = russianLossesSender;
        }

        public void InitAndStart(List<SubscriptionData> subscribers)
        {
            _subscribers = subscribers;
        }

        public void AddChat(long chatId, byte hour, byte minute)
        {
            if (_subscribers.Any(c => c.ChatId == chatId))
            {
                RemoveChat(chatId);
            }
            _subscribers.Add(new SubscriptionData(chatId, true, hour, minute));
        }

        public void RemoveChat(long chatId)
        {
            //// Find the chat to remove
            //var chatToRemove = _activeChats.FirstOrDefault(c => c.chatId == chatId);

            //// Check if the chat was found
            //if (chatToRemove != default)
            //{
            //    // Remove the chat from the list
            //    _activeChats.Remove(chatToRemove);
            //}
        }

        protected override async Task ExecuteAsync(CancellationToken cts)
        {            // TODO: task should wait untill InitAndStart()
            //DateTime kyivReportTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 12, 0, 0);
            //TimeZoneInfo kyivTimeZone = TimeZoneInfo.FindSystemTimeZoneById("E. Europe Standard Time");
            //DateTime _reportTime = TimeZoneInfo.ConvertTimeToUtc(kyivReportTime, kyivTimeZone);

            //DateTime now = DateTime.UtcNow;
            //if (now > _reportTime)
            //{
            //    _reportTime = _reportTime.AddDays(1);
            //}
            //var delay = _reportTime - now;
            //await Task.Delay(delay, cts);
            //using PeriodicTimer timer = new PeriodicTimer(TimeSpan.FromDays(1));

            //while (!cts.IsCancellationRequested)
            //{
            //    System.Diagnostics.Trace.WriteLine("RussianLossesDailyReport");
            //    var losses = await _russianLossesService.GetData();
            //    //TODO: add time parametes handling
            //    foreach (var chat in _activeChats)
            //    {
            //        await _russianLossesSender.Send(losses, chat.chatId);
            //    }
            //    await timer.WaitForNextTickAsync();
            //}
        }
    }
}
