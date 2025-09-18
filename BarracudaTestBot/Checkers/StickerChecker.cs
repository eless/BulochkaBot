namespace BarracudaTestBot.Checkers;

public class StickerChecker
{
    ///TODO Move to db
    private readonly Dictionary<string, List<string>> _stickersByCommand = new()
    {
        ["русні пизда"] = ["CAACAgIAAxkBAAECwVZoyzfTCbl6x_6uDNoB5rJNyyHTcQAC0R0AAtz9eUiSMtzqMNIUsjYE"],
        ["ктоплатит"] = ["CAACAgQAAxkBAAECwLpoyzJUNuXfZ0GHFjCFzCrbAwfdBgACJwMAAkII_FGDd3c9ThGw1zYE"],
        ["остановитесь"] = ["CAACAgQAAxkBAAECwMJoyzNJMg17hXAqkwRQVF-rWmHb7AACcQIAAtoNfFGcjIWto_eqBDYE"],
        ["тривога"] =
                [
                    "CAACAgQAAxkBAAECwMZoyzNqoOV02OEp4hELoXZdUHXSkwACkAMAAnhP9FFxat0YYJy0JjYE",
                    "CAACAgQAAxkBAAECwMpoyzOD2ZDNqKyOCuvNakb63qJVjgACJAMAAgNJJFNgFKR-DaHV7DYE",
                    "CAACAgQAAxkBAAECwM5oyzOiIfcsYb0G0Ejp96LIq6YSowACaAMAAvwa_VGHajyjvtbd2TYE",
                ],
        ["відбій"] =
                [
                    "CAACAgQAAxkBAAECwNJoyzPHI-wzCrN_fxuxxHBGMW5FqQACggMAAqVg_VEG5parv_aJGjYE",
                    "CAACAgQAAxkBAAECwNZoyzPLMtS_OhXTAkYZ0nhnVVm10AAClgMAAtDGhVLhDyHzGOsI8DYE",
                    "CAACAgQAAxkBAAECwL5oyzJ6lWtJTOot7LuZCZZses85rgACtQMAAqtL_FEh41f-Ih1lPDYE",
                ]
    };

    public IEnumerable<string> GetCommands() => _stickersByCommand.Keys;

    public bool IsStickerCommand(string command) => _stickersByCommand.ContainsKey(command);

    public string GetStickerLink(string command)
    {
        if (_stickersByCommand.TryGetValue(command, out var stickerLinks))
        {
            var rnd = new Random();
            var stickerLink = stickerLinks?.OrderBy(s => rnd.Next()).FirstOrDefault();
            return stickerLink!;
        }
        return string.Empty;
    }
}
