using Telegram.Bot.Types.Enums;

namespace BarracudaTestBot.Services;

public interface ICommandAnswer
{
    string Text { get; set; }
    ParseMode ParseMode { get; set; }
}

public class CommandAnswer: ICommandAnswer
{
    public CommandAnswer(string text, ParseMode parseMode) => (Text, ParseMode) = (text, parseMode);

    public string Text { get; set; }

    public ParseMode ParseMode { get; set; }
}

