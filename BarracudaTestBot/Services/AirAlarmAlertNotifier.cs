using Telegram.Bot;
using Telegram.Bot.Types;
using Microsoft.ApplicationInsights;

namespace BarracudaTestBot.Services
{
    public class AirAlarmAlertNotifier(ITelegramBotClient botClient, AirAlarmStickerSelector stickerSelector, TelemetryClient telemetry)
    {
        private readonly ITelegramBotClient _botClient = botClient;
        private readonly AirAlarmStickerSelector _stickerSelector = stickerSelector;

        public async void Send()
        {
            var fileId = _stickerSelector.GetAlertSticker();
            try
            {
                await _botClient.SendStickerAsync(
                    chatId: -1001344803304,
                    sticker: InputFile.FromFileId(fileId));
                // TODO: add text yobana rusnia or get it from some generator 
                // maybe add info about time between current alert and last all clear

            } catch(Exception ex)
            {
                telemetry.TrackTrace($"Alert, sticker send failed, id: {fileId}");
                telemetry.TrackException(ex);
            }
        }
    }
}
