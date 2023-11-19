using Telegram.Bot;

namespace BarracudaTestBot.Services
{
    public class AirAlarmAllClearNotifier
    {
        private TelegramBotClient _botClient;
        private AirAlarmStickerSelector _stickerSelector;
        public AirAlarmAllClearNotifier(ITelegramBotClient botClient, AirAlarmStickerSelector stickerSelector)
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
                    sticker: _stickerSelector.GetAllClearSticker());
                // TODO: add text with alert duration
            } catch (Exception ex)
            {

            }

        }
    }
}
