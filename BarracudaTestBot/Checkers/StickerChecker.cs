namespace BarracudaTestBot.Checkers;

public class StickerChecker
{
    private Dictionary<string, List<string>> _stickersByCommand = new Dictionary<string, List<string>>
    {
        ["русні пизда"] = new List<string> { "CAACAgIAAxkBAAEBZWljTVG3uiQ6EpmPJNLPCMQYqgHKpAAC0R0AAtz9eUiSMtzqMNIUsioE" },
        ["ктоплатит"] = new List<string> { "https://sticker-collection.com/stickers/plain/vosem_let/512/7c81d17f-6aac-40fd-bba3-e283348b1c9afile_1910543.webp" },
        ["остановитесь"] = new List<string> { "https://tlgrm.ru/_/stickers/230/5c9/2305c9a3-dd7a-37b3-b38c-27e99d652dc2/2.webp" },
        ["тривога"] = new List<string>
                {
                    "https://sticker-collection.com/stickers/plain/Povitryana_tryvoha/512/e739064c-be9d-4787-a628-8bb7939dec54file_1875815.webp",
                    "https://sticker-collection.com/stickers/plain/Povitryana_tryvoha/512/acee37c2-7cea-49de-9ee6-b7cda696b7d3file_1875819.webp",
                    "https://sticker-collection.com/stickers/plain/Povitryana_tryvoha/512/2bc666cc-5de7-41c9-941b-4b6f5f084e58file_1875825.webp"
                },
        ["відбій"] = new List<string>
                {
                    "https://sticker-collection.com/stickers/plain/Povitryana_tryvoha/512/d9ce940c-e68a-4ae0-b069-518634eff356file_1875818.webp",
                    "https://sticker-collection.com/stickers/plain/Povitryana_tryvoha/512/f9d0b34c-c8db-4c5c-af66-fd90ab25f601file_1875820.webp",
                    "https://sticker-collection.com/stickers/plain/Povitryana_tryvoha/512/f2d6625a-44e1-4736-9945-0a3450b3db55file_1875826.webp"
                }
    };

    public bool IsStickerCommand(string command) => _stickersByCommand.ContainsKey(command);

    public string GetStickerLink(string command)
    {
        _stickersByCommand.TryGetValue(command, out List<string> stickerLinks);
        var rnd = new Random();
        var stickerLink = stickerLinks?.OrderBy(s => rnd.Next()).FirstOrDefault();
        return stickerLink ?? string.Empty;
    }
}
