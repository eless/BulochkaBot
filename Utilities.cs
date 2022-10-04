using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using Telegram.Bot;
using Telegram.Bot.Requests.Abstractions;

namespace BarracudaTestBot
{
    public static class Utilities
    {
        public static async Task<Message> SendText(this ITelegramBotClient botClient,
            int? replyTo,
            long chatId,
            string text,
            CancellationToken cancellationToken) => 
                await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: text,
                    parseMode: ParseMode.MarkdownV2,
                    replyToMessageId: replyTo,
                    cancellationToken: cancellationToken);

    }
}
