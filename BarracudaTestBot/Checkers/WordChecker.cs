using System.Text.RegularExpressions;
using BarracudaTestBot.Services;
using Telegram.Bot.Types.Enums;

namespace BarracudaTestBot.Checkers;

public class WordChecker
{
    private readonly StickerChecker _stickerChecker;
    private RussianLossesService _russianLossesService;
    private PutinGenerator _putinGenerator;

    public WordChecker(RussianLossesService russianLossesService, PutinGenerator putinGenerator, StickerChecker stickerChecker) =>
        (_russianLossesService, _putinGenerator, _stickerChecker) = (russianLossesService, putinGenerator, stickerChecker);

    protected Dictionary<Regex, CommandAnswer> Commands => new Dictionary<Regex, CommandAnswer>
    {
        [new Regex("^Слава Україні!$")] = new CommandAnswer("*Героям слава\\!*", ParseMode.MarkdownV2),
        [new Regex("шо по русні", RegexOptions.IgnoreCase)] = new CommandAnswer("*русні пизда\\!*", ParseMode.MarkdownV2),
        [new Regex("путін", RegexOptions.IgnoreCase)] = new CommandAnswer(_putinGenerator.GenerateName(), ParseMode.Markdown),
        [new Regex("булочка", RegexOptions.IgnoreCase)] = new CommandAnswer("мурняв", ParseMode.Markdown),
        [new Regex($"^/losses")] = new CommandAnswer(_russianLossesService.GetData().Result, ParseMode.MarkdownV2),
        [new Regex($"^/stickers")] = new CommandAnswer(string.Join(Environment.NewLine, _stickerChecker.GetCommands()), ParseMode.MarkdownV2),
    };

    public IEnumerable<CommandAnswer> GetAnswersByCommand(string command)
    {
        var commands = Commands.Where(c => c.Key.IsMatch(command));

        return commands.Select(c => c.Value);
    }
}
