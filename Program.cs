using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using Microsoft.Extensions.Configuration;
using BarracudaTestBot;

Console.OutputEncoding = System.Text.Encoding.UTF8;

var builder = new ConfigurationBuilder()
            .AddJsonFile($"appsettings.json", false, true)
            .AddJsonFile($"appsettings.Local.json", true, true)
            .AddEnvironmentVariables();
var configuration = builder.Build();
var token = configuration.GetValue<string>("TelegramToken");

var botClient = new TelegramBotClient(token);

using var cts = new CancellationTokenSource();

var StickersByCommand = new Dictionary<string, string>
{
    ["остановитесь"] = "https://tlgrm.ru/_/stickers/230/5c9/2305c9a3-dd7a-37b3-b38c-27e99d652dc2/2.webp"
};

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

Console.WriteLine($"Start listening for @{me.Username}");
Console.ReadLine();

// Send cancellation request to stop bot
cts.Cancel();

async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
{
    // Only process Message updates: https://core.telegram.org/bots/api#message
    if (update.Message is not { } message)
        return;
    // Only process text messages
    if (message.Text is not { } messageText)
        return;

    var chatId = message.Chat.Id;
    Console.WriteLine($"Received a '{messageText}' message in chat {chatId}.");

    var commandsList = messageText.Split(' ', StringSplitOptions.RemoveEmptyEntries);

    if (messageText == "Слава Україні!")
    {
        await botClient.SendText(message.ReplyToMessage?.MessageId, chatId, "*Героям слава\\!*", cancellationToken);
    }
    else if (messageText.ToLower().Contains("путін"))
    {
        await botClient.SendText(message.ReplyToMessage?.MessageId, chatId, "*путін ХУЙЛО\\! Ла ла ла ла ла ла ла ла*", cancellationToken);
    }

    if (commandsList[0] != "бот") return;

    if (commandsList.Length > 1 && StickersByCommand.TryGetValue(commandsList[1], out string? stickerLink))
    {
        await botClient.SendStickerAsync(
            chatId: chatId,
            sticker: stickerLink,
            cancellationToken: cancellationToken);
        return;
    }

    var correctMessageText = messageText.Remove(0, 3).TrimStart();

    var answer = message.ReplyToMessage?.MessageId != null ? $"у відповідь:" : "";

    var text = $"@{message.From.Username} {answer} {correctMessageText}";

    await botClient.DeleteMessageAsync(chatId, message.MessageId);
    await botClient.SendText(message.ReplyToMessage?.MessageId, chatId, text, cancellationToken);
}

Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
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
