using BarracudaTestBot.Database;
using Microsoft.EntityFrameworkCore;

namespace BarracudaTestBot.Repositories;

public class AiChatMessageRepository(IDbContextFactory<BotDbContext> dbContextFactory)
{
    public const int MaxHistoryDepth = 20;
    private static readonly TimeSpan Retention = TimeSpan.FromDays(30);

    public async Task<IReadOnlyList<AiChatMessage>> GetReplyChainAsync(
        long chatId, int replyToMessageId, CancellationToken cancellationToken)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        var chain = new List<AiChatMessage>();
        int? currentId = replyToMessageId;

        while (currentId is int messageId && chain.Count < MaxHistoryDepth)
        {
            var message = await db.AiChatMessages
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    m => m.ChatId == chatId && m.MessageId == messageId,
                    cancellationToken);

            if (message is null) break;

            chain.Add(message);
            currentId = message.ReplyToMessageId;
        }

        chain.Reverse();
        return chain;
    }

    public async Task SaveExchangeAsync(
        AiChatMessage userMessage, AiChatMessage assistantMessage, CancellationToken cancellationToken)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        db.AiChatMessages.Add(userMessage);
        db.AiChatMessages.Add(assistantMessage);

        var cutoff = DateTimeOffset.UtcNow - Retention;
        await db.AiChatMessages
            .Where(m => m.CreatedAt < cutoff)
            .ExecuteDeleteAsync(cancellationToken);

        await db.SaveChangesAsync(cancellationToken);
    }
}
