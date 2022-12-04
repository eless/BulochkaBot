using Telegram.Bot;

namespace BarracudaTestBot.Services
{
    public class AirAlarmAllClearNotifier
    {
        private TelegramBotClient _botClient;
        private AirAlarmStickerSelector _stickerSelector;
        AirAlarmAllClearNotifier(ITelegramBotClient botClient, AirAlarmStickerSelector stickerSelector)
        {
            _botClient = (TelegramBotClient) botClient;
            _stickerSelector = stickerSelector;
        }

        public async void Send()
        {
            try
            {
                await _botClient.SendStickerAsync(
                    chatId: -1001344803304,
                    sticker: _stickerSelector.GetSticker());
                // TODO: add text with alert duration
            }
            catch
            {

            }
        }
    }
}
