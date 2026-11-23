using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using Telegram.Bot;
using BarracudaTestBot.Checkers;
using BarracudaTestBot.Database;
using BarracudaTestBot.Repositories;
using Telegram.Bot.Exceptions;
using Microsoft.ApplicationInsights;

namespace BarracudaTestBot.Services;

public class BotService(WordChecker wordChecker, StickerChecker stickerChecker, ITelegramBotClient botClient,
                  RussianLossesSender russianLossesSender, RussianLossesService russianLossesService,
                  TelemetryClient telemetry, AiChatService aiChat, AiChatMessageRepository aiChatMessages)
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
            AllowedUpdates = [] // receive all update types
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
        try
        {
            await HandleUpdateCoreAsync(update, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            // An exception escaping the update handler stops Telegram.Bot polling for good,
            // so every failure must be contained here.
            telemetry.TrackTrace($"UPDATE HANDLING FAILED: {ex.Message}");
            telemetry.TrackException(ex);
        }
    }

    private async Task HandleUpdateCoreAsync(Update update, CancellationToken cancellationToken)
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
            await SendCommandAnswers(commandAnswers, cancellationToken, message);
        }

        if (stickerChecker.IsStickerCommand(messageText))
        {
            await botClient.DeleteMessageAsync(chatId, message.MessageId, cancellationToken);
            var fileId = stickerChecker.GetStickerLink(messageText);
            try
            {
                await botClient.SendStickerAsync(
                    chatId: chatId,
                    sticker: InputFile.FromFileId(fileId),
                    replyToMessageId: message.ReplyToMessage?.MessageId,
                    cancellationToken: cancellationToken);
            } catch(Exception ex)
            {
                telemetry.TrackTrace($"Bot Service, sticker {messageText} send failed, id: {fileId}");
                telemetry.TrackException(ex);
            }
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

    private async Task SendCommandAnswers(IEnumerable<ICommandAnswer> commandAnswers, CancellationToken cancellationToken, Message? message = null)
    {
        var tasks = commandAnswers.Select(async commandText =>
        {
            try
            {
                if (commandText.Text == "булочка" && message != null)
                {
                    await SendAIAnswer(message, cancellationToken);
                    return;
                }
                if (commandText.Text == "losses")
                {
                    var data = await russianLossesService.GetData(cancellationToken);
                    await russianLossesSender.Send(data, message?.Chat?.Id ?? -1001344803304);
                }
                else if (stickerChecker.IsStickerCommand(commandText.Text))
                {
                    var fileId = stickerChecker.GetStickerLink(commandText.Text);
                    try
                    {
                        await botClient.SendStickerAsync(
                            chatId: message?.Chat?.Id ?? -1001344803304,
                            sticker: InputFile.FromFileId(fileId),
                            cancellationToken: cancellationToken);

                    } catch(Exception ex)
                    {
                        telemetry.TrackTrace($"Bot Service, command {commandText.Text} send failed, id: {fileId}");
                        telemetry.TrackException(ex);
                    }
                }
                else
                {
                    await SendText(
                        message?.ReplyToMessage?.MessageId,
                        message?.Chat?.Id ?? -1001344803304,
                        commandText.Text,
                        cancellationToken,
                        commandText.ParseMode);
                }
            }
            catch (Exception e)
            {
                var context = message is null
                    ? "message=null"
                    : FormatAiMessageContext(message);
                telemetry.TrackTrace($"COMMAND FAILED: {e.Message}; {context}");
                telemetry.TrackException(e);
                throw;
            }
        });

        await Task.WhenAll(tasks);
    }

    private async Task SendAIAnswer(Message message, CancellationToken cancellationToken)
    {
        if (!aiChat.HasProviders || message.Text is null) return;

        var nowUtc = DateTimeOffset.UtcNow;
        var systemPrompt = $"Ти кішка з ім'ям Булочка. Відповідай як кішка, але технічно коректно. Поточний час (UTC): {nowUtc:yyyy-MM-dd HH:mm:ss}. Вважай цю дату та час поточними і не вигадуй інший час. Проте пиши дату в повідомленні. лише у випадках, коли тебе прямо просять про це.";

        var authorName = FormatTelegramAuthorName(message.From);

        var conversation = new List<AiConversationTurn>();

        int? historyReplyToMessageId = null;
        if (message.ReplyToMessage?.MessageId is int replyToMessageId)
        {
            try
            {
                var history = await aiChatMessages.GetReplyChainAsync(
                    message.Chat.Id, replyToMessageId, cancellationToken);

                foreach (var item in history)
                {
                    conversation.Add(new AiConversationTurn
                    {
                        Role = item.Role switch
                        {
                            "assistant" => AiConversationRole.Assistant,
                            _ => AiConversationRole.User
                        },
                        Content = item.Text,
                        AuthorName = item.AuthorName
                    });
                }

                // History is non-empty only when the replied-to message itself was stored.
                if (history.Count > 0)
                    historyReplyToMessageId = replyToMessageId;
            }
            catch (Exception ex)
            {
                telemetry.TrackTrace(
                    $"AI history load failed: {ex.Message}; {FormatAiMessageContext(message)}");
                telemetry.TrackException(ex);
            }
        }

        conversation.Add(new AiConversationTurn
        {
            Role = AiConversationRole.User,
            Content = message.Text,
            AuthorName = authorName
        });

        // Null means every provider is out of quota or failed; the bot just stays quiet.
        var answer = await aiChat.CompleteAsync(systemPrompt, conversation, cancellationToken);
        if (answer == null) return;

        var replyText = string.IsNullOrWhiteSpace(authorName) ? answer : $"{authorName}, {answer}";
        // Plain text: model answers often contain *, _, etc. that break Telegram Markdown and
        // prevent SaveExchangeAsync from running, which then starts a new chain on the next reply.
        var telegramText = TruncateTelegramText(replyText);

        Message sent;
        try
        {
            sent = await SendText(
                message.MessageId,
                message.Chat.Id,
                telegramText,
                cancellationToken,
                parseMode: null);
        }
        catch (Exception ex)
        {
            telemetry.TrackTrace(
                $"AI answer send failed: {ex.Message}; {FormatAiMessageContext(message)}; answerLength={replyText.Length}");
            telemetry.TrackException(ex);
            return;
        }

        try
        {
            var createdAt = DateTimeOffset.UtcNow;
            await aiChatMessages.SaveExchangeAsync(
                new AiChatMessage
                {
                    ChatId = message.Chat.Id,
                    MessageId = message.MessageId,
                    ReplyToMessageId = historyReplyToMessageId,
                    Role = "user",
                    Text = message.Text,
                    AuthorName = authorName,
                    CreatedAt = createdAt
                },
                new AiChatMessage
                {
                    ChatId = message.Chat.Id,
                    MessageId = sent.MessageId,
                    ReplyToMessageId = message.MessageId,
                    Role = "assistant",
                    Text = replyText,
                    AuthorName = null,
                    CreatedAt = createdAt
                },
                cancellationToken);
        }
        catch (Exception ex)
        {
            telemetry.TrackTrace(
                $"AI history save failed: {ex.Message}; {FormatAiMessageContext(message)}; sentMessageId={sent.MessageId}; historyReplyToMessageId={historyReplyToMessageId?.ToString() ?? "null"}");
            telemetry.TrackException(ex);
        }
    }

    private static string FormatAiMessageContext(Message message) =>
        $"chatId={message.Chat.Id}; messageId={message.MessageId}; replyToMessageId={message.ReplyToMessage?.MessageId?.ToString() ?? "null"}";

    private static string? FormatTelegramAuthorName(User? user) => user switch
    {
        null => null,
        { Username.Length: > 0 } => $"@{user.Username}",
        { FirstName.Length: > 0 } => user.FirstName,
        _ => null
    };

    private const int TelegramMaxMessageLength = 4096;

    private static string TruncateTelegramText(string text) =>
        text.Length <= TelegramMaxMessageLength
            ? text
            : text[..(TelegramMaxMessageLength - 1)] + "…";

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
