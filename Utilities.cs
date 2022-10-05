using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using Telegram.Bot;

namespace BarracudaTestBot
{
    public static class Utilities
    {
        public static async Task<Message> SendText(this ITelegramBotClient botClient,
            int? replyTo,
            long chatId,
            string text,
            CancellationToken cancellationToken, ParseMode? parseMode = ParseMode.MarkdownV2) => 
                await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: text,
                    parseMode: parseMode,
                    replyToMessageId: replyTo,
                    cancellationToken: cancellationToken);

        public static int GetRandomNumber(int from, int to) => new Random().Next(from, to);

    }
}
