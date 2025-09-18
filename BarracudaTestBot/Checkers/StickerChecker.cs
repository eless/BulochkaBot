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
                    "CAACAgIAAxkBAAECwXpoy54YnpKAK1rlyxtSMn3NoxHjggACGRgAAo1z2Uo25GNgK3tadDYE",
                    "CAACAgIAAxkBAAECwYJoy54dEoijrMR17HnDzotH9cHolAAC6xgAArpDeEkS8aDRxkTaezYE",
                    "CAACAgIAAxkBAAECwYpoy54k5Wth_rsQk_xglMjT3gQQ5wAC4hgAAm7heUnHD4DFeiep7TYE",
                ],
        ["відбій"] =
                [
                    "CAACAgIAAxkBAAECwX5oy54aAf37llN0IIDwH7wSfmns_gACkxUAAo7_4UpAXuQxTq2p0zYE",
                    "CAACAgIAAxkBAAECwYZoy54gKcvNSFYQRvztkfUP3xSBzwACsRcAAkV4gUltA6qv5jdGtjYE",
                    "CAACAgIAAxkBAAECwY5oy54mM9ms4b_LDLuk6kxaMLPFVAACzRcAAkCReUmw2Y-R4Z09MTYE",
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
