using BarracudaTestBot.Database;
using Microsoft.EntityFrameworkCore;

namespace BarracudaTestBot.Repositories;

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

    public async Task Unsubscribe(long chatId)
    {
        var subscription = await dbContext.RussianLossesSubscriptions
            .Where(entity => entity.ChatId == chatId)
            .ToListAsync();
        if (subscription != default)
        {
            dbContext.RussianLossesSubscriptions.RemoveRange(subscription);
        }
        await dbContext.SaveChangesAsync();
    }
    public async Task<RussianLossesSubscription?> GetLossesSubscription(long chatId) =>
        await dbContext.RussianLossesSubscriptions
            .FirstOrDefaultAsync(entity => entity.ChatId == chatId);

    public IAsyncEnumerable<RussianLossesSubscription> GetAllLossesSubscriptions() =>
        dbContext.RussianLossesSubscriptions.AsAsyncEnumerable();
}
