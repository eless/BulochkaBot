using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BarracudaTestBot.Services
{
    public class RussianLossesSender(ITelegramBotClient botClient)
    {
        private readonly ITelegramBotClient _botClient = botClient;

        // debug chat id: 512242748
        public async Task Send(RussianLossesData data, long chatId = -1001344803304)
        {
            if (!string.IsNullOrEmpty(data.units))
            {
                await _botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: data.units,
                    parseMode: ParseMode.MarkdownV2);
            }
            foreach(var sticker in data.stickers.Where(s => !string.IsNullOrEmpty(s))) {
                await _botClient.SendStickerAsync(
                        chatId: chatId,
                    sticker: InputFile.FromUri(sticker));
            }
            foreach (var gif in data.animations.Where(g => !string.IsNullOrEmpty(g)))
            {
                await _botClient.SendAnimationAsync(
                        chatId: chatId,
                    animation: InputFile.FromUri(gif));
            }
        }
    }
}
