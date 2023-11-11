namespace BarracudaTestBot.Services
{
    public class RussianLossesSubscriptionService
    {
        private RussianLossesSubscriptionDataBase _dataBase;
        private RussianLossesDailyReport _dailyReport;

        public RussianLossesSubscriptionService(RussianLossesSubscriptionDataBase dataBase, RussianLossesDailyReport dailyReport)
        {
            _dataBase = dataBase;
            _dailyReport = dailyReport;
            var subscribedChats = _dataBase.GetAllLossesSubscriptions();
            _dailyReport.InitAndStart(subscribedChats);
        }

        public void Subscribe(long chatId, byte hour, byte minute)
        {
            _dataBase.Subscribe(chatId, true, hour, minute);
            _dailyReport.AddChat(chatId, hour, minute);
        }

        public void Unsubscribe(long chatId)
        {
            _dataBase.Unsubscribe(chatId);
            _dailyReport.RemoveChat(chatId);
        }

        public (bool subscribed, byte hour, byte minute) GetSubscriptionInfo(long chatId)
        {
            var res = _dataBase.GetLossesSubscription(chatId);
            return (res.Subscribed, res.Hour, res.Minute);
        }

        public List<SubscriptionData> GetAllSubscriptionInfo(long chatId)
        {
            var result = _dataBase.GetAllLossesSubscriptions();
            return result;
        }
    }
}