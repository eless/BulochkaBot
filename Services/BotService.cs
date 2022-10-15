using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using Telegram.Bot;
using BarracudaTestBot.Checkers;
using Telegram.Bot.Exceptions;

namespace BarracudaTestBot.Services;

public class BotService
{
    private static DateTime _dateOfStart = DateTime.UtcNow;

    private readonly string _token;
    public BotService(IConfiguration configuration)
    {
        _token = configuration.GetValue<string>("TelegramToken");
    }

    public async Task Start(CancellationTokenSource cts)
    {
        var botClient = new TelegramBotClient(_token);

        List<long> MutedInChats = new List<long>();

        var Commands = new List<BotCommand>();
        var command = new BotCommand();
        command.Command = "off"; command.Description = "вимкнути бота в чаті";
        Commands.Add(command);
        command = new BotCommand();
        command.Command = "on"; command.Description = "ввімкнути бота в чаті";
        Commands.Add(command);

        await botClient.SetMyCommandsAsync(Commands, cancellationToken: cts.Token);
        // StartReceiving does not block the caller thread. Receiving is done on the ThreadPool.
        var receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = Array.Empty<UpdateType>() // receive all update types
        };
        botClient.StartReceiving(
            updateHandler: HandleUpdateAsync,
            pollingErrorHandler: HandlePollingErrorAsync,
            receiverOptions: receiverOptions,
            cancellationToken: cts.Token
        );

        var me = await botClient.GetMeAsync();

        System.Diagnostics.Trace.WriteLine($"Start listening for @{me.Username}");
        //Console.ReadLine();

        while (true) ;
    }

    private static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        if (update.Message?.Date < _dateOfStart) return;

        // Only process Message updates: https://core.telegram.org/bots/api#message
        if (update.Message is not { } message)
            return;
        // Only process text messages
        if (message.Text is not { } messageText)
            return;

        var chatId = message.Chat.Id;
        System.Diagnostics.Trace.WriteLine($"Received a '{messageText}' message in chat {chatId}.");

        var commandsList = messageText.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        var checkedWord = new WordChecker().GetAnswerByCommand(messageText);

        if (!string.IsNullOrEmpty(checkedWord))
        {
            try
            {
                await botClient.SendText(message.ReplyToMessage?.MessageId, chatId, checkedWord, cancellationToken);
            }
            catch (ApiRequestException)
            {
                await botClient.SendText(message.ReplyToMessage?.MessageId, chatId, checkedWord, cancellationToken, null);
            }
            return;
        }

        if (commandsList[0] != "бот") return;

        var stickerSender = new StickerChecker();
        if (commandsList.Length > 1 && stickerSender.IsStickerCommand(commandsList[1]))
        {
            await botClient.SendStickerAsync(
                chatId: chatId,
                sticker: stickerSender.GetStickerLink(commandsList[1]),
                cancellationToken: cancellationToken);
            return;
        }

        var correctMessageText = messageText.Remove(0, 3).TrimStart();

        var answer = message.ReplyToMessage?.MessageId != null ? $"у відповідь:" : "";

        var text = $"@{message.From.Username} {answer} {correctMessageText}";

        await botClient.DeleteMessageAsync(chatId, message.MessageId);
        await botClient.SendText(message.ReplyToMessage?.MessageId, chatId, text, cancellationToken, null);
    }

    private static Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        var ErrorMessage = exception switch
        {
            ApiRequestException apiRequestException
                => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => exception.ToString()
        };

        Console.WriteLine(ErrorMessage);
        return Task.CompletedTask;
    }

}
