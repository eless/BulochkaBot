using BarracudaTestBot.Database;

namespace BarracudaTestBot.Repositories
{
    public class RussianLossesSubscriptionRepository(BotDbContext dbContext)
    {

        public void Subscribe(RussianLossesSubscription subscription)
        {
            var existingSubscription = dbContext.RussianLossesSubscriptions
                .SingleOrDefault(entity => entity.ChatId == subscription.ChatId);
            if (existingSubscription == default)
            {
                dbContext.RussianLossesSubscriptions.Add(subscription);
                dbContext.SaveChanges();
            }
        }

        public void Unsubscribe(RussianLossesSubscription subscription)
        {
            dbContext.RussianLossesSubscriptions.Remove(subscription);
            dbContext.SaveChanges();
        }

        public RussianLossesSubscription? GetLossesSubscription(long chatId) =>
            dbContext.RussianLossesSubscriptions
                .FirstOrDefault(entity => entity.ChatId == chatId);

        public List<RussianLossesSubscription> GetAllLossesSubscriptions() => [.. dbContext.RussianLossesSubscriptions];
    }
}
