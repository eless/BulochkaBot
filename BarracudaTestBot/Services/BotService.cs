using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using Telegram.Bot;
using BarracudaTestBot.Checkers;
using Telegram.Bot.Exceptions;
using Microsoft.ApplicationInsights;
using System.Collections.Concurrent;

namespace BarracudaTestBot.Services;

public class BotService(WordChecker wordChecker, StickerChecker stickerChecker, ITelegramBotClient botClient,
                  RussianLossesSender russianLossesSender, RussianLossesService russianLossesService,
                  TelemetryClient telemetry)
{
    private readonly DateTime _dateOfStart = DateTime.UtcNow;

    List<long> MutedInChats { get; set; } = new List<long>();

    Dictionary<string, string> Commands = new Dictionary<string, string>
    {
        ["off"] = "вимкнути бота в чаті",
        ["on"] = "ввімкнути бота в чаті",
        ["losses"] = "втрати підарасні на сьогодні",
        ["stickers"] = "команди стікерів"
    };

    public async Task Start(CancellationTokenSource cts)
    {
        var commands = new List<BotCommand>();
        foreach (var commandKVP in Commands)
        {
            var command = new BotCommand();
            command.Command = commandKVP.Key; command.Description = commandKVP.Value;
            commands.Add(command);
        }
        await botClient.SetMyCommandsAsync(commands, cancellationToken: cts.Token);
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

        if (messageText == $"/off")
        {
            MutedInChats.Add(chatId);
        } else if (messageText == $"/on")
        {
            MutedInChats.Remove(chatId);
        }
        if (MutedInChats.Contains(chatId)) return;

        var commandAnswers = wordChecker.GetAnswersByCommand(messageText);

        if (commandAnswers != null)
        {
            SendCommandAnswers(commandAnswers, cancellationToken, message);
        }

        if (stickerChecker.IsStickerCommand(messageText))
        {
            await botClient.DeleteMessageAsync(chatId, message.MessageId, cancellationToken);
            await botClient.SendStickerAsync(
                chatId: chatId,
                sticker: InputFile.FromUri(stickerChecker.GetStickerLink(messageText)),
                replyToMessageId: message.ReplyToMessage?.MessageId,
                cancellationToken: cancellationToken);
            return;
        }

        var commandsList = messageText.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (commandsList[0] != "бот") return;

        var correctMessageText = messageText.Remove(0, 3).TrimStart();

        var answer = message.ReplyToMessage?.MessageId != null ? $"у відповідь: " : "";

        var text = $"@{message!.From?.Username} {answer}{correctMessageText}";

        await botClient.DeleteMessageAsync(chatId, message!.MessageId, cancellationToken);
        await SendText(message.ReplyToMessage?.MessageId, chatId, text, cancellationToken, null);
    }

    private void SendCommandAnswers(IEnumerable<CommandAnswer> commandAnswers, CancellationToken cancellationToken, Message? message = null)
    {
        var exceptions = new ConcurrentQueue<Exception>();

        commandAnswers
            .AsParallel()
                .ForAll(
                    async (commandText) =>
                    {
                        try
                        {
                            if (commandText.Text == "losses")
                            {
                                var data = await russianLossesService.GetData(cancellationToken);
                                await russianLossesSender.Send(data, message?.Chat?.Id ?? -1001344803304);
                            }
                            else
                            if (stickerChecker.IsStickerCommand(commandText.Text))
                                await botClient.SendStickerAsync(
                                    chatId: message?.Chat?.Id ?? -1001344803304,
                                    sticker: InputFile.FromUri(stickerChecker.GetStickerLink(commandText.Text)),
                                    cancellationToken: cancellationToken);
                            else
                                await SendText(
                                    message?.ReplyToMessage?.MessageId,
                                    message?.Chat?.Id ?? -1001344803304,
                                    commandText.Text,
                                    cancellationToken,
                                    commandText.ParseMode);
                        }
                        catch (Exception e)
                        {
                            exceptions.Enqueue(e);
                        }
                    }
                );

        // Throw the exceptions here after the loop completes.
        if (!exceptions.IsEmpty)
        {
            throw new AggregateException(exceptions);
        }
    }

    private async Task<Message> SendText(int? replyTo, long chatId, string text,
        CancellationToken cancellationToken, ParseMode? parseMode = ParseMode.MarkdownV2) =>
            await botClient.SendTextMessageAsync(
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
        Console.WriteLine(ErrorMessage);
        telemetry.TrackTrace(ErrorMessage);
        return Task.CompletedTask;
    }

}
