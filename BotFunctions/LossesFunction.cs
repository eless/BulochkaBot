using BarracudaTestBot.Checkers;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;

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

        [FunctionName("LossesFunction")]
        public async Task Run([TimerTrigger("0 0 9 * * *")]TimerInfo myTimer, ILogger log)
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
