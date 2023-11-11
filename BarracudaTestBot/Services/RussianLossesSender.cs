using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace BarracudaTestBot.Services
{
    public class RussianLossesSender
    {
        private ITelegramBotClient _botClient;
        
        public RussianLossesSender(ITelegramBotClient botClient)
        {
            _botClient = botClient;
        }

        public async Task Send(RussianLossesData data, long chatId)
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
                    sticker: sticker);
            }
            foreach (var gif in data.animations.Where(g => !string.IsNullOrEmpty(g)))
            {
                await _botClient.SendAnimationAsync(
                        chatId: chatId,
                    animation: gif);
            }
        }
    }
}
