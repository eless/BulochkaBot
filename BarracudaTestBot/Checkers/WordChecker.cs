using System.Text.RegularExpressions;
using BarracudaTestBot.Services;
using Telegram.Bot.Types.Enums;

namespace BarracudaTestBot.Checkers;

public class WordChecker
{
    private readonly StickerChecker _stickerChecker;
    private RussianLossesService _russianLossesService;
    private PutinGenerator _putinGenerator;
    private RussianLossesSender _russianLossesSender;

    public WordChecker(RussianLossesService russianLossesService, PutinGenerator putinGenerator, StickerChecker stickerChecker, RussianLossesSender russianLossesSender) =>
        (_russianLossesService, _putinGenerator, _stickerChecker, _russianLossesSender) = (russianLossesService, putinGenerator, stickerChecker, russianLossesSender);

    protected Dictionary<Regex, Action<List<CommandAnswer>>> Commands => new Dictionary<Regex, Action<List<CommandAnswer>>>
    {
        [new Regex("^Слава Україні!$")] = (commands) => commands.Add(new CommandAnswer("*Героям слава\\!*", ParseMode.MarkdownV2)),
        [new Regex("шо по русні", RegexOptions.IgnoreCase)] = (commands) => commands.Add(new CommandAnswer("*русні пизда\\!*", ParseMode.MarkdownV2)),
        [new Regex("путін", RegexOptions.IgnoreCase)] = (commands) => commands.Add(new CommandAnswer(_putinGenerator.GenerateName(), ParseMode.Markdown)),
        [new Regex("булочка", RegexOptions.IgnoreCase)] = (commands) => commands.Add(new CommandAnswer("мурняв", ParseMode.Markdown)),
        [new Regex($"^/losses")] = (commands) =>
        {
            var losses = _russianLossesService.GetData().Result;
            _russianLossesSender.Send(losses);
        },
        [new Regex($"^/stickers")] = (commands) => commands.Add(new CommandAnswer(string.Join(Environment.NewLine, _stickerChecker.GetCommands()), ParseMode.MarkdownV2)),
    };

    public IEnumerable<CommandAnswer> GetAnswersByCommand(string command)
    {
        var commandsList = new List<CommandAnswer>();
        Commands
            .Where(c => c.Key.IsMatch(command))
            .ToList().ForEach(action => action.Value(commandsList));

        return commandsList;
    }
}
