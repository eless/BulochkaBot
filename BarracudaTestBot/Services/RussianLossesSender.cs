using BarracudaTestBot.Checkers;
using System.Threading;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace BarracudaTestBot.Services
{
    public class RussianLossesSender
    {
        private ITelegramBotClient _botClient;
        
        public RussianLossesSender(ITelegramBotClient botClient)
        {
            _botClient = botClient;
        }

        public async Task Send(RussianLossesData data)
        {
            if (!string.IsNullOrEmpty(data.units))
            {
                await _botClient.SendTextMessageAsync(
                    chatId: -1001344803304,
                    text: data.units);
            }
            foreach(var sticker in data.stickers) {
                if (!string.IsNullOrEmpty(sticker))
                {
                    await _botClient.SendStickerAsync(
                        chatId: -1001344803304,
                        sticker: sticker);
                }
            }
            foreach (var gif in data.animations)
            {
                if (!string.IsNullOrEmpty(gif))
                {
                    await _botClient.SendAnimationAsync(
                        chatId: -1001344803304,
                        animation: gif);
                }
            }
        }
    }
}
