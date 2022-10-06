using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using Microsoft.Extensions.Configuration;
using BarracudaTestBot;

Console.OutputEncoding = System.Text.Encoding.UTF8;

var dateOfStart = DateTime.UtcNow;

var builder = new ConfigurationBuilder()
            .AddJsonFile($"appsettings.json", false, true)
            .AddJsonFile($"appsettings.Local.json", true, true)
            .AddEnvironmentVariables();
var configuration = builder.Build();
var token = configuration.GetValue<string>("TelegramToken");

var botClient = new TelegramBotClient(token);

using var cts = new CancellationTokenSource();

var StickersByCommand = new Dictionary<string, List<string>>
{
    ["ктоплатит"] = new List<string> { "https://sticker-collection.com/stickers/plain/vosem_let/512/7c81d17f-6aac-40fd-bba3-e283348b1c9afile_1910543.webp" },
    ["остановитесь"] = new List<string> { "https://tlgrm.ru/_/stickers/230/5c9/2305c9a3-dd7a-37b3-b38c-27e99d652dc2/2.webp" },
    ["тривога"]= new List<string>
                {
                    "https://sticker-collection.com/stickers/plain/Povitryana_tryvoha/512/e739064c-be9d-4787-a628-8bb7939dec54file_1875815.webp",
                    "https://sticker-collection.com/stickers/plain/Povitryana_tryvoha/512/acee37c2-7cea-49de-9ee6-b7cda696b7d3file_1875819.webp"
                },
    ["відбій"] = new List<string>
                {
                    "https://sticker-collection.com/stickers/plain/Povitryana_tryvoha/512/d9ce940c-e68a-4ae0-b069-518634eff356file_1875818.webp",
                    "https://sticker-collection.com/stickers/plain/Povitryana_tryvoha/512/f9d0b34c-c8db-4c5c-af66-fd90ab25f601file_1875820.webp"
                }
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
    if (update.Message?.Date < dateOfStart) return;

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

    if (commandsList.Length > 1 && StickersByCommand.TryGetValue(commandsList[1], out List<string> stickerLinks))
    {
        var rnd = new Random();
        var stickerLink = stickerLinks.OrderBy(s => rnd.Next()).First();
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
    await botClient.SendText(message.ReplyToMessage?.MessageId, chatId, text, cancellationToken, null);
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
