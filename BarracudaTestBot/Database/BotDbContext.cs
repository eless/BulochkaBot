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
    public DbSet<AiChatMessage> AiChatMessages { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AiChatMessage>(entity =>
        {
            entity.HasKey(e => new { e.ChatId, e.MessageId });
            entity.Property(e => e.Role).HasMaxLength(16);
            entity.HasIndex(e => e.CreatedAt);
        });
    }
}
