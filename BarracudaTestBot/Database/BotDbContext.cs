using Microsoft.EntityFrameworkCore;

namespace BarracudaTestBot.Database
{
    public class BotDbContext : DbContext
    {
        public BotDbContext() : base()
        {
        }

        public BotDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<RussianLossesSubscription> RussianLossesSubscription { get; set; }
    }
}
