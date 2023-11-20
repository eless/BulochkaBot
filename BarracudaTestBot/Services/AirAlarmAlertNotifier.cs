using Telegram.Bot;
using Telegram.Bot.Types;

namespace BarracudaTestBot.Services
{
    public class AirAlarmAlertNotifier(ITelegramBotClient botClient, AirAlarmStickerSelector stickerSelector)
    {
        private readonly ITelegramBotClient _botClient = botClient;
        private readonly AirAlarmStickerSelector _stickerSelector = stickerSelector;

        public async void Send()
        {
            await _botClient.SendStickerAsync(
                chatId: -1001344803304,
                sticker: InputFile.FromUri(_stickerSelector.GetAlertSticker()));
            // TODO: add text yobana rusnia or get it from some generator 
            // maybe add info about time between current alert and last all clear
        }
    }
}
