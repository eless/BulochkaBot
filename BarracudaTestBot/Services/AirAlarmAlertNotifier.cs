using BarracudaTestBot.Checkers;
using System.Threading;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace BarracudaTestBot.Services
{
    public class AirAlarmAlertNotifier
    {
        private TelegramBotClient _botClient;
        private AirAlarmStickerSelector _stickerSelector;
        AirAlarmAlertNotifier(ITelegramBotClient botClient, AirAlarmStickerSelector stickerSelector)
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
                // TODO: add text yobana rusnia or get it from some generator 
                // maybe add info about time between current allert and last all clear
            }
            catch
            {

            }
        }
    }
}
