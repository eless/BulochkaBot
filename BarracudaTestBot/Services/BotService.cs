using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using Telegram.Bot;
using BarracudaTestBot.Checkers;
using Telegram.Bot.Exceptions;
using Microsoft.ApplicationInsights;
using OpenAI.Chat;
using System.Collections.Concurrent;

namespace BarracudaTestBot.Services;

public class BotService(WordChecker wordChecker, StickerChecker stickerChecker, ITelegramBotClient botClient,
                  RussianLossesSender russianLossesSender, RussianLossesService russianLossesService,
                  TelemetryClient telemetry, IConfiguration configuration)
{
    private readonly DateTime _dateOfStart = DateTime.UtcNow;

    ChatClient client = new(
      model: "gpt-4.1",
      apiKey: configuration.GetValue<string>("OPENAI_API_KEY")
    );

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
                    await SendAIAnswer(commandText, message, cancellationToken);
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
                telemetry.TrackTrace($"COMMAND FAILED: {e.Message}");
                telemetry.TrackException(e);
                throw;
            }
        });

        await Task.WhenAll(tasks);
    }

    private async Task SendAIAnswer(ICommandAnswer commandText, Message? message, CancellationToken cancellationToken)
    {
        var nowUtc = DateTimeOffset.UtcNow;
        var answer = await client.CompleteChatAsync(
        [   new SystemChatMessage($"Ти кішка з ім'ям Булочка. Відповідай як кішка, але технічно коректно. Поточний час (UTC): {nowUtc:yyyy-MM-dd HH:mm:ss}. Вважай цю дату та час поточними і не вигадуй інший час. Проте пиши дату в повідомленні. лише у випадках, коли тебе прямо просять про це."),
            new UserChatMessage(message!.Text)
        ], cancellationToken: cancellationToken);
        await SendText(
            null,
            message?.Chat?.Id ?? -1001344803304,
            answer.Value.Content[0].Text,
            cancellationToken,
            commandText.ParseMode);
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
