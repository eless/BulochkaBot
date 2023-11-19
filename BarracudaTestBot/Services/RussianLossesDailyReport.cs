using System.Collections.Concurrent;

namespace BarracudaTestBot.Services
{
    public class RussianLossesDailyReport
    {
        private interface IMessage
        {
        }

        private class SchedulerMessage : IMessage
        {

        }

        private class SubscribeMessage : IMessage
        {
            public SubscribeMessage(SubscriptionData subscription)
            {
                NewSub = subscription;
            }
            public SubscriptionData NewSub;
        }

        private class UnsubscribeMessage : IMessage
        {
            public UnsubscribeMessage(long chatId)
            {
                ChatID = chatId;
            }

            public long ChatID { get; }
        }

        private RussianLossesService _russianLossesService;
        private RussianLossesSender _russianLossesSender;
        private List<SubscriptionData> _subscribers;
        private RussianLossesSubscriptionDataBase _dataBase;
        private BlockingCollection<IMessage> messageQueue = new BlockingCollection<IMessage>();
        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private Timer _scheduler;

        public RussianLossesDailyReport(RussianLossesService russianLossesService, RussianLossesSender russianLossesSender,
                                        RussianLossesSubscriptionDataBase dataBase)
        {
            _russianLossesService = russianLossesService;
            _russianLossesSender = russianLossesSender;
            _dataBase = dataBase;
            Task.Run(() => ProcessMessagesAsync(cancellationTokenSource.Token), cancellationTokenSource.Token);
        }

        private void SendMessage(IMessage message)
        {
            messageQueue.Add(message);
        }

        public void OnSubscribe(long chatId, byte hour, byte minute)
        {
            var sub = new SubscribeMessage(new SubscriptionData(chatId, true, hour, minute));
            SendMessage(sub);
        }

        public void OnUnsubscribe(long chatId)
        {
            var unsubscribeMessage = new UnsubscribeMessage(chatId);
            SendMessage(unsubscribeMessage);
        }

        public SubscriptionData GetSubscriptionInfo(long chatId)
        {
            return _dataBase.GetLossesSubscription(chatId);
        }

        public void Stop()
        {
            cancellationTokenSource.Cancel();
        }

        private void ScheduleTimeCheck(object state)
        {
            var checkMsg = new SchedulerMessage();
            SendMessage(checkMsg);
        }

        private DateTime GetCurrentKyivTime()
        {
            TimeZoneInfo kyivTimeZone = TimeZoneInfo.FindSystemTimeZoneById("E. Europe Standard Time");
            DateTime kyivTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, kyivTimeZone);
            return kyivTime;
        }

        private async Task ProcessMessagesAsync(CancellationToken cancellationToken)
        {
            _scheduler = new Timer(ScheduleTimeCheck, null, 0, (int)TimeSpan.FromMinutes(1).TotalMilliseconds);

            try
            {
                // Init of the subscribers base at the start.
                _subscribers = _dataBase.GetAllLossesSubscriptions();
                // Cache losses data at the start.
                RussianLossesData losses = await _russianLossesService.GetData();

                while (!cancellationToken.IsCancellationRequested)
                {
                    IMessage message = messageQueue.Take(cancellationToken);

                    // Process the message based on its type
                    if (message is SchedulerMessage)
                    {
                        var kyivTime = GetCurrentKyivTime();
                        if (kyivTime.Hour == 10 && kyivTime.Minute == 00)
                        {
                            losses = await _russianLossesService.GetData(); // cache new data once a day
                        }
                        foreach (var subscriber in _subscribers)
                        {
                            if (subscriber.Hour == kyivTime.Hour && subscriber.Minute == kyivTime.Minute)
                            {
                                await _russianLossesSender.Send(losses, subscriber.ChatId);
                            }
                        }
                    }
                    else if (message is SubscribeMessage sub)
                    {
                        _dataBase.Subscribe(sub.NewSub);
                        var existingSubscriber = _subscribers.FirstOrDefault(oldSub => oldSub.ChatId == sub.NewSub.ChatId);
                        if (existingSubscriber != null)
                        {
                            int index = _subscribers.IndexOf(existingSubscriber);
                            _subscribers[index] = sub.NewSub;
                        }
                        else
                        {
                            _subscribers.Add(sub.NewSub);
                        }
                    }
                    else if (message is UnsubscribeMessage unsub)
                    {
                        _dataBase.Unsubscribe(unsub.ChatID);
                        _subscribers.RemoveAll(subscriber => subscriber.ChatId == unsub.ChatID);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // TODO: Handle cancellation
            }
            catch (Exception ex)
            {
                // TODO: Handle Exception
                Console.WriteLine($"Unexpected error in ProcessMessagesAsync: {ex.Message}");
            }
        }
    }
}
