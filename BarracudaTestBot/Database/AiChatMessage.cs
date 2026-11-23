namespace BarracudaTestBot.Database;

public class AiChatMessage
{
    public long ChatId { get; set; }

    public int MessageId { get; set; }

    public int? ReplyToMessageId { get; set; }

    /// <summary>Stored as "user" or "assistant".</summary>
    public required string Role { get; set; }

    public required string Text { get; set; }

    public string? AuthorName { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}
