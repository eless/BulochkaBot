using BarracudaTestBot.Checkers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Microsoft.Azure.Functions.Worker;

namespace BotFunctions
{
    public class LossesFunction
    {
        private readonly IConfiguration configuration;
        private readonly WordChecker wordChecker;

        public LossesFunction(IConfiguration configuration, WordChecker wordChecker)
        {
            this.configuration = configuration;
            this.wordChecker = wordChecker;
        }

        [Function("LossesFunction")]
        public async Task Run([TimerTrigger("0 0 6 * * *")]TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"LossesFunction executed at: {DateTime.Now}");

            var token = configuration.GetValue<string>("TelegramToken");
            var botClient = new TelegramBotClient(token);
            if (wordChecker.GetAnswersByCommand("/losses").FirstOrDefault() is not { } message)
            {
                return;
            }
            await botClient.SendTextMessageAsync(
                chatId: -1001344803304,
                text: message.Text,
                parseMode: message.ParseMode,
                cancellationToken: CancellationToken.None);
        }
    }
}
