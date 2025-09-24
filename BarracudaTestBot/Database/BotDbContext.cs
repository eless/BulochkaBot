using Microsoft.EntityFrameworkCore;

namespace BarracudaTestBot.Database;

public class BotDbContext : DbContext
{
    public BotDbContext() : base()
    {
    }

    public BotDbContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<RussianLossesSubscription> RussianLossesSubscriptions { get; set; }
    public DbSet<Sticker> Stickers { get; set; }
}
