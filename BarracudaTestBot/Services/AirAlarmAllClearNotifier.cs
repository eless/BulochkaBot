using Telegram.Bot;
using Telegram.Bot.Types;

namespace BarracudaTestBot.Services
{
    public class AirAlarmAllClearNotifier(ITelegramBotClient botClient, AirAlarmStickerSelector stickerSelector)
    {
        private TelegramBotClient _botClient = (TelegramBotClient)botClient;
        private AirAlarmStickerSelector _stickerSelector = stickerSelector;

        public async void Send() => await _botClient.SendStickerAsync(
                chatId: -1001344803304,
                sticker: InputFile.FromFileId(_stickerSelector.GetAllClearSticker()));// TODO: add text with alert duration
    }
}
