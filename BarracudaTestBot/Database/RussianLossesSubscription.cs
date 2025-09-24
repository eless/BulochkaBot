namespace BarracudaTestBot.Database;
public class RussianLossesSubscription
{
    public int Id { get; set; }

    public long ChatId { get; set; }

    public short? Hour { get; set; }

    public short? Minutes { get; set; }
}
