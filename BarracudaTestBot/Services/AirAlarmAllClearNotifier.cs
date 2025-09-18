using Telegram.Bot;
using Telegram.Bot.Types;
using Microsoft.ApplicationInsights;

namespace BarracudaTestBot.Services
{
    public class AirAlarmAllClearNotifier(ITelegramBotClient botClient, AirAlarmStickerSelector stickerSelector,
                                          TelemetryClient telemetry)
    {
        private TelegramBotClient _botClient = (TelegramBotClient)botClient;
        private AirAlarmStickerSelector _stickerSelector = stickerSelector;
        private TelemetryClient _telemetry = telemetry;

        public async void Send()
        {
            var fileId = _stickerSelector.GetAllClearSticker();
            try
            {
                await _botClient.SendStickerAsync(
                    chatId: -1001344803304,
                    sticker: InputFile.FromFileId(fileId));
                // TODO: add text yobana rusnia or get it from some generator 
                // maybe add info about time between current alert and last all clear

            } catch(Exception ex)
            {
                _telemetry.TrackTrace($"AllClear, sticker send failed, id: {fileId}");
                _telemetry.TrackException(ex);
            }
        }
    }
}
