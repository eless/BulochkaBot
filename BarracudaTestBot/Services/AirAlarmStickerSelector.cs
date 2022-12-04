namespace BarracudaTestBot.Services
{
    public class AirAlarmStickerSelector
    {
        private List<String> alertStickers = new List<String> {
            "https://sticker-collection.com/stickers/plain/Povitryana_tryvoha/512/e739064c-be9d-4787-a628-8bb7939dec54file_1875815.webp",
            "https://sticker-collection.com/stickers/plain/Povitryana_tryvoha/512/acee37c2-7cea-49de-9ee6-b7cda696b7d3file_1875819.webp",
            "https://sticker-collection.com/stickers/plain/Povitryana_tryvoha/512/2bc666cc-5de7-41c9-941b-4b6f5f084e58file_1875825.webp"
        };

        private List<String> allClearStickers = new List<String> {
            "https://sticker-collection.com/stickers/plain/Povitryana_tryvoha/512/d9ce940c-e68a-4ae0-b069-518634eff356file_1875818.webp",
            "https://sticker-collection.com/stickers/plain/Povitryana_tryvoha/512/f9d0b34c-c8db-4c5c-af66-fd90ab25f601file_1875820.webp",
            "https://sticker-collection.com/stickers/plain/Povitryana_tryvoha/512/f2d6625a-44e1-4736-9945-0a3450b3db55file_1875826.webp"
        };

        private int lastAlertStickerIndex = 0;

        public string GetSticker()
        {
            // if (alert) lastAlertStickerIndex = random(); return alertStickers(lastAlertStickerIndex) ;
            // if (allClear) return allClearStickers(lastAlertStickerIndex);
            return alertStickers[lastAlertStickerIndex];
        }
        //TODO: add collection for alert and all clear stickers.
        //Add random alert sticker selection, save last alert sticker index and return the same index from all clear collection.
        //Depending on which class called GetSticker the corresponding sticker links should be returned.
    }
}
