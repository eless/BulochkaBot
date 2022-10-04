using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using Microsoft.Extensions.Configuration;

Console.OutputEncoding = System.Text.Encoding.UTF8;

var builder = new ConfigurationBuilder()
            .AddJsonFile($"appsettings.json", false, true)
            .AddJsonFile($"appsettings.Local.json", true, true)
            .AddEnvironmentVariables();
var configuration = builder.Build();
var token = configuration.GetValue<string>("TelegramToken");

var botClient = new TelegramBotClient(token);

using var cts = new CancellationTokenSource();

Dictionary<string, string> StickersByCommand = new Dictionary<string, string>
{
    ["остановитесь"] = "https://tlgrm.ru/_/stickers/230/5c9/2305c9a3-dd7a-37b3-b38c-27e99d652dc2/2.webp"
};



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

    if(messageText == "Слава Україні!")
    {
        await botClient.SendTextMessageAsync(
            chatId: chatId,
            text: "*Героям слава\\!*",
            parseMode: ParseMode.MarkdownV2,
            replyToMessageId: message.ReplyToMessage?.MessageId,
            cancellationToken: cancellationToken);
    } else if (messageText.ToLower().Contains("путін")) {
            await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: "*путін ХУЙЛО\\! Ла ла ла ла ла ла ла ла*",
                parseMode: ParseMode.MarkdownV2,
                replyToMessageId: message.ReplyToMessage?.MessageId,
                cancellationToken: cancellationToken);

    }

    var commandsList = message.Text.Split(' ', StringSplitOptions.RemoveEmptyEntries);

    if (commandsList[0] != "бот") return;
/*
    if(message.From.Username == "i_sirius")
    {
        await botClient.SendTextMessageAsync(
            chatId: chatId,
            text: $"@{message.From.Username} дає пізди всім.",
            replyToMessageId: message.ReplyToMessage?.MessageId,
            cancellationToken: cancellationToken);

    }
*/
    if (commandsList.Length > 1 && StickersByCommand.TryGetValue(commandsList[1], out string? stickerLink))
    {
        await botClient.SendStickerAsync(
            chatId: chatId,
            sticker: stickerLink,
            cancellationToken: cancellationToken);
        return;
    }

    var correctMessageText = messageText.Remove(0, 3).TrimStart();

    Console.WriteLine($"Received a '{messageText}' message in chat {chatId}.");
    var answer = message.ReplyToMessage?.MessageId != null ? $"у відповідь:": "";

    var text = $"@{message.From.Username} {answer} {correctMessageText}";
    // Echo received message text
    Message sentMessage = await botClient.SendTextMessageAsync(
        chatId: chatId,
        text: text,
        replyToMessageId: message.ReplyToMessage?.MessageId,
        cancellationToken: cancellationToken);
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
