
using System.Text.RegularExpressions;
using PtnGen;
using RusLosses;

namespace BarracudaTestBot.Checkers;

public class WordChecker
{
    protected Dictionary<Regex, Func<string>> Commands => new Dictionary<Regex, Func<string>>
    {
        [new Regex("^Слава Україні!$")] = () => "*Героям слава\\!*",
        [new Regex("^шо по русні", RegexOptions.IgnoreCase)] = () => "*русні пизда\\!*",
        [new Regex("путін", RegexOptions.IgnoreCase)] = () => new PutinGenerator().GenerateName(),
        [new Regex($"^/losses")] = () => new Losses().GetData().Result,
    };

    public string GetAnswerByCommand(string command)
    {
        var commands = Commands.Where(c => c.Key.IsMatch(command));
        
        if (commands.Count() == 0) return string.Empty;

        return commands.First().Value.Invoke();
    }
}
