
using System.Text.RegularExpressions;

namespace BarracudaTestBot.Checkers;

public class WordChecker
{
    private Dictionary<Regex, Func<string>> _commands = new Dictionary<Regex, Func<string>>
    {
        [new Regex("^Слава Україні!$")] = () => "*Героям слава\\!*",
        [new Regex("путін", RegexOptions.IgnoreCase)] = () => "*путін ХУЙЛО\\! Ла ла ла ла ла ла ла ла*"
    };

    public string GetAnswerByCommand(string command)
    {
        var key = _commands.Keys.Where(c => c.IsMatch(command)).FirstOrDefault();
        
        if (key == null) return string.Empty;

        return _commands[key].Invoke();
    }
}
