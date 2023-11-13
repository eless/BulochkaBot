using System.Collections.Concurrent;

namespace BarracudaTestBot.Services
{
    public class RussianLossesDailyReport
    {
        private interface IMessage
        {
        }

        private class SubscribeMessage : IMessage
        {
            public SubscribeMessage(long chatId, byte hour, byte minute)
            {
                ChatId = chatId;
                Hour = hour;
                Minute = minute;
            }

            public long ChatId { get; }
            public byte Hour { get; }
            public byte Minute { get; }
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
            var sub = new SubscribeMessage(chatId, hour, minute);
            SendMessage(sub);
        }

        public void OnUnsubscribe(long chatId)
        {
            var unsubscribeMessage = new UnsubscribeMessage(chatId);
            SendMessage(unsubscribeMessage);
        }

        public void Stop()
        {
            cancellationTokenSource.Cancel();
        }

        private async Task ProcessMessagesAsync(CancellationToken cancellationToken)
        {
            try
            {
                // Init of the subscribers base at the start.
                _subscribers = _dataBase.GetAllLossesSubscriptions();

                while (!cancellationToken.IsCancellationRequested)
                {
                    IMessage message = messageQueue.Take(cancellationToken);

                    // Process the message based on its type
                    if (message is SubscribeMessage sub)
                    {
                        _dataBase.Subscribe(sub.ChatId , true, sub.Hour, sub.Minute);
                        // TODO: add to the _subscribers cache, reinit timers and send logic
                    }
                    else if (message is UnsubscribeMessage unsub)
                    {
                        _dataBase.Unsubscribe(unsub.ChatID);
                        // TODO: remove from the _subscribers cache, reinit timers and send logic
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

// Previous code for the reference TODO: remove when not needed.
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