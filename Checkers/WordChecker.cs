using System.Text.RegularExpressions;
using BarracudaTestBot.Services;
using PtnGen;
using RusLosses;
using Telegram.Bot.Types.Enums;

namespace BarracudaTestBot.Checkers;

public class WordChecker
{
    protected Dictionary<Regex, Func<CommandAnswer>> Commands => new Dictionary<Regex, Func<CommandAnswer>>
    {
        [new Regex("^Слава Україні!$")] = () => new CommandAnswer("*Героям слава\\!*", ParseMode.MarkdownV2),
        [new Regex("шо по русні", RegexOptions.IgnoreCase)] = () => new CommandAnswer("*русні пизда\\!*", ParseMode.MarkdownV2),
        [new Regex("путін", RegexOptions.IgnoreCase)] = () => new CommandAnswer(new PutinGenerator().GenerateName(), ParseMode.Markdown),
        [new Regex("булочка", RegexOptions.IgnoreCase)] = () => new CommandAnswer("мурняв", ParseMode.Markdown),
        [new Regex($"^/losses")] = () => new CommandAnswer(new Losses().GetData().Result, ParseMode.MarkdownV2),
    };

    public IEnumerable<CommandAnswer> GetAnswerByCommand(string command)
    {
        var commands = Commands.Where(c => c.Key.IsMatch(command));

        return commands.Select(c => c.Value.Invoke());
    }
}
