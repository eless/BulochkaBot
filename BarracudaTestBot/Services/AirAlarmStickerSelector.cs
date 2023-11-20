namespace BarracudaTestBot.Services
{
    public class AirAlarmStickerSelector
    {
        // Alert and AllClear stickers are grouped in pairs by the actual picture on the sticker, the order should not change.
        // If alertStickers[0] is used to notify about the alert the index 0 sticker should also be used for AllClear notification.
        // This class should not be used anywhere else except AirAlarmAlertNotifier or AirAlarmAllClearNotifier classes.
        // New stickers should be added also as pairs, and sticker count in both lists should always match.

        private readonly List<string> _alertStickers =
        [
            "CAACAgIAAxkBAAEBY3xjSeEE269IAAGUAwAB65HFXDSyZV7tAALoIAACUmeJSJLkdz0x4VKBKgQ",
            "CAACAgIAAxkBAAEBY4RjSeFQxyJpVnlEpBqQsulZ0C7j-wACwx8AAixxiEjoHSQ48whpRyoE",
            "CAACAgIAAxkBAAEBY4xjSeGAkKBg1DYdukPboYTkgCgfBQACWBoAAg_ckUgeLguTYm4kMSoE",
            "CAACAgIAAxkBAAEBY5RjSeHJiVSmT8dJPDYpjKkEQwotnQAC4h0AAokviUjltHCHPC78LCoE",
            "CAACAgIAAxkBAAEBgGBjjlzhe4RWJ6ECMOC205vbmQ2XlAACGRgAAo1z2Uo25GNgK3tadCsE"
        ];

        private readonly List<string> _allClearStickers =
        [
            "CAACAgIAAxkBAAEBY4BjSeE1Cw88viOoFf4Mkk0Dv44o_wAC_RwAAuURiEggQmSHj7Cb4yoE",
            "CAACAgIAAxkBAAEBY4hjSeFoCJfPgNuXN-1Dksc0vOjMkQACix8AAgwRiUjPeFGsKPjPISoE",
            "CAACAgIAAxkBAAEBY5BjSeG2NTerE9_gd3MIuI70xmHnkwACZyEAAuegiEimsxDXD0wemyoE",
            "CAACAgIAAxkBAAEBY5hjSeHY_3Sz8uqFeoQCcTgcQ9VrfwACdiIAAqDLiEhFekZT5nY2DSoE",
            "CAACAgIAAxkBAAEBgGRjjlz54IsngAJP6DUu3I_FK1TFzQACkxUAAo7_4UpAXuQxTq2p0ysE"
        ];

        private int _lastAlertStickerIndex = 0;

        public string GetAlertSticker()
        {
            var random = new Random();
            _lastAlertStickerIndex = random.Next(_alertStickers.Count);
            return _alertStickers[_lastAlertStickerIndex];
        }

        public string GetAllClearSticker()
        {
            return _allClearStickers[_lastAlertStickerIndex];
        }
    }
}
