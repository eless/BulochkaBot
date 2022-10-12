
using System.Text.RegularExpressions;
using PtnGen;
using RusLosses;

namespace BarracudaTestBot.Checkers;

public class WordChecker
{
    private static PutinGenerator puiulo = new PutinGenerator();
    private static Losses losses = new Losses();

    private Dictionary<Regex, Func<string>> _commands = new Dictionary<Regex, Func<string>>
    {
        [new Regex("^Слава Україні!$")] = () => "*Героям слава\\!*",
        [new Regex("путін", RegexOptions.IgnoreCase)] = () => puiulo.GetName(),
        [new Regex("^/losses$")] = () => losses.GetData().Result,
    };

    public string GetAnswerByCommand(string command)
    {
        var key = _commands.Keys.Where(c => c.IsMatch(command)).FirstOrDefault();
        
        if (key == null) return string.Empty;

        return _commands[key].Invoke();
    }
}
