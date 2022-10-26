using Telegram.Bot.Types.Enums;
using static System.Net.Mime.MediaTypeNames;

namespace BarracudaTestBot.Services
{
    public class CommandAnswer
    {
        public CommandAnswer(string text, ParseMode parseMode) => (Text, ParseMode) = (text, parseMode);

        public string Text { get; set; }

        public ParseMode ParseMode { get; set; }
    }
}
