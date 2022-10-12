
using System.Text.RegularExpressions;
using PtnGen;
using RusLosses;

namespace BarracudaTestBot.Checkers;

public class WordChecker
{
    private Dictionary<Regex, Func<string>> _commands = new Dictionary<Regex, Func<string>>
    {
        [new Regex("^Слава Україні!$")] = () => "*Героям слава\\!*",
        [new Regex("путін", RegexOptions.IgnoreCase)] = () => new PutinGenerator().GenerateName(),
        [new Regex("^/losses$")] = () => new Losses().GetData().Result,
    };

    public string GetAnswerByCommand(string command)
    {
        var key = _commands.Keys.Where(c => c.IsMatch(command)).FirstOrDefault();
        
        if (key == null) return string.Empty;

        return _commands[key].Invoke();
    }
}
