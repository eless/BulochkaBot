using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using Telegram.Bot;
using BarracudaTestBot.Checkers;
using Telegram.Bot.Exceptions;
using Microsoft.ApplicationInsights;
using System.Globalization;

namespace BarracudaTestBot.Services;

public class BotService
{
    private readonly DateTime _dateOfStart = DateTime.UtcNow;
    private readonly ITelegramBotClient _botClient;
    private readonly WordChecker _wordChecker;
    private readonly StickerChecker _stickerChecker;
    private RussianLossesSender _russianLossesSender;
    private RussianLossesService _russianLossesService;
    private TelemetryClient _telemetry;
    private RussianLossesDailyReport _lossesDailyService;

    List<long> MutedInChats { get; set; } = new List<long>();

    Dictionary<string, string> Commands = new Dictionary<string, string>
    {
        ["off"] = "вимкнути бота в чаті",
        ["on"] = "ввімкнути бота в чаті",
        ["losses"] = "втрати підарасні на сьогодні",
        ["stickers"] = "команди стікерів",
        ["losses_subscribe"] = "ввімкнути щоденну розсилку втрат русні в чаті",
        ["losses_unsubscribe"] = "вимкнути щоденну втрат русні в чаті",
    };

    public BotService(WordChecker wordChecker, StickerChecker stickerChecker, ITelegramBotClient botClient,
                      RussianLossesSender russianLossesSender, RussianLossesService russianLossesService,
                      TelemetryClient telemetry, RussianLossesDailyReport lossesSubscriptionManager)
    {
        _botClient = botClient;
        _wordChecker = wordChecker;
        _stickerChecker = stickerChecker;
        _russianLossesSender = russianLossesSender;
        _russianLossesService = russianLossesService;
        _telemetry = telemetry;
        _lossesDailyService = lossesSubscriptionManager;
    }

    public async Task Start(CancellationTokenSource cts)
    {
        var commands = new List<BotCommand>();
        foreach (var commandKVP in Commands)
        {
            var command = new BotCommand();
            command.Command = commandKVP.Key; command.Description = commandKVP.Value;
            commands.Add(command);
        }
        await _botClient.SetMyCommandsAsync(commands, cancellationToken: cts.Token);
        // StartReceiving does not block the caller thread. Receiving is done on the ThreadPool.
        var receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = Array.Empty<UpdateType>() // receive all update types
        };
        _botClient.StartReceiving(
            updateHandler: HandleUpdateAsync,
            pollingErrorHandler: HandlePollingErrorAsync,
            receiverOptions: receiverOptions,
            cancellationToken: cts.Token
        );

        var me = await _botClient.GetMeAsync();

        System.Diagnostics.Trace.WriteLine($"Start listening for @{me.Username}");
    }

    private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
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
        Console.WriteLine($"Received a '{messageText}' message in chat {chatId}.");

        if (messageText.Contains("/losses_subscribe"))
        {
            string[] parts = messageText.Split(' ');
            byte hour = 12;
            byte minute = 00;
            if (parts.Length == 2)
            {
                var timeParameter = parts[1];
                if (DateTime.TryParseExact(timeParameter, "HH:mm", CultureInfo.InvariantCulture, 
                    DateTimeStyles.None, out DateTime parsedTime))
                {
                    hour = (byte)parsedTime.Hour;
                    minute = (byte)parsedTime.Minute;
                }
            }
            _lossesDailyService.OnSubscribe(chatId, hour, minute);
            var str = $"Щоденна статистика втрат русні буде приходити о {hour:D2}:{minute:D2} 😺";
            await SendText(message.ReplyToMessage?.MessageId, chatId, str, cancellationToken, null);
            return;
        } else if (messageText == "/losses_unsubscribe")
        {
            _lossesDailyService.OnUnsubscribe(chatId);
            var str = $"Ви відписались від щоденної розсилки втрат русні 😾";
            await SendText(message.ReplyToMessage?.MessageId, chatId, str, cancellationToken, null);
            return;
        }
        else if (messageText == $"/off")
        {
            MutedInChats.Add(chatId);
        } else if(messageText == $"/on")
        {
            MutedInChats.Remove(chatId);
        }
        if (MutedInChats.Contains(chatId)) return;

        var commandAnswers = _wordChecker.GetAnswersByCommand(messageText);

        if (commandAnswers != null)
        {
            SendCommandAnswers(commandAnswers, cancellationToken, message);
        }

        if (_stickerChecker.IsStickerCommand(messageText))
        {
            await botClient.DeleteMessageAsync(chatId, message.MessageId);
            await botClient.SendStickerAsync(
                chatId: chatId,
                sticker: _stickerChecker.GetStickerLink(messageText),
                replyToMessageId: message.ReplyToMessage?.MessageId,
                cancellationToken: cancellationToken);
            return;
        }

        var commandsList = messageText.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (commandsList[0] != "бот") return;

        var correctMessageText = messageText.Remove(0, 3).TrimStart();

        var answer = message.ReplyToMessage?.MessageId != null ? $"у відповідь: " : "";

        var text = $"@{message?.From?.Username} {answer}{correctMessageText}";

        await botClient.DeleteMessageAsync(chatId, message.MessageId);
        await SendText(message.ReplyToMessage?.MessageId, chatId, text, cancellationToken, null);
    }

    private void SendCommandAnswers(IEnumerable<CommandAnswer> commandAnswers, CancellationToken cancellationToken, Message? message = null) =>
        commandAnswers
                .AsParallel()
                .ForAll(
                    async (commandText) =>
                    {
                        if (commandText.Text == "losses")
                        {
                            var data = await _russianLossesService.GetData();
                            await _russianLossesSender.Send(data, message?.Chat?.Id ?? -1001344803304);
                        } else
                        if (_stickerChecker.IsStickerCommand(commandText.Text))
                            await _botClient.SendStickerAsync(
                                chatId: message?.Chat?.Id ?? -1001344803304,
                                sticker: _stickerChecker.GetStickerLink(commandText.Text),
                                cancellationToken: cancellationToken);
                        else
                            await SendText(
                                message?.ReplyToMessage?.MessageId,
                                message?.Chat?.Id ?? -1001344803304,
                                commandText.Text,
                                cancellationToken,
                                commandText.ParseMode);
                    });

    private async Task<Message> SendText(int? replyTo, long chatId, string text,
        CancellationToken cancellationToken, ParseMode? parseMode = ParseMode.MarkdownV2) =>
            await _botClient.SendTextMessageAsync(
                chatId: chatId,
                text: text,
                parseMode: parseMode,
                replyToMessageId: replyTo,
                cancellationToken: cancellationToken);

    private Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        var ErrorMessage = exception switch
        {
            ApiRequestException apiRequestException
                => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => exception.ToString()
        };

        _telemetry.TrackTrace(ErrorMessage);
        return Task.CompletedTask;
    }

}
