using BarracudaTestBot.Database;
using Microsoft.EntityFrameworkCore;

namespace BarracudaTestBot.Repositories
{
    public class RussianLossesSubscriptionRepository(BotDbContext dbContext)
    {

        public async Task Subscribe(RussianLossesSubscription subscription)
        {
            var existingSubscription = await dbContext.RussianLossesSubscriptions
                .SingleOrDefaultAsync(entity => entity.ChatId == subscription.ChatId);
            if (existingSubscription == default)
            {
                await dbContext.RussianLossesSubscriptions.AddAsync(subscription);
            }
            else
            {
                existingSubscription.Hour = subscription.Hour;
                existingSubscription.Minutes = subscription.Minutes;
            }
            await dbContext.SaveChangesAsync();
        }

        public async Task Unsubscribe(RussianLossesSubscription subscription)
        {
            dbContext.RussianLossesSubscriptions.Remove(subscription);
            await dbContext.SaveChangesAsync();
        }

        public RussianLossesSubscription? GetLossesSubscription(long chatId) =>
            dbContext.RussianLossesSubscriptions
                .FirstOrDefault(entity => entity.ChatId == chatId);

        public async Task<List<RussianLossesSubscription>> GetAllLossesSubscriptions() => await dbContext.RussianLossesSubscriptions.ToListAsync();
    }
}
