using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using Telegram.Bot;

namespace BarracudaTestBot
{
    public static class Utilities
    {
        public static int GetRandomNumber(int from, int to) => new Random().Next(from, to);

    }

}
