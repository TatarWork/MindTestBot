using Microsoft.EntityFrameworkCore;
using MindBot.EF.Entities;

namespace MindBot.EF
{
    /// <summary>
    /// Контекст базы данных
    /// </summary>
    public class MindBotDbContext : DbContext
    {
        /// <summary>
        /// Таблица UserStates (состояния пользователей)
        /// </summary>
        public DbSet<UserStateEntity> UserStates { get; set; }

        public MindBotDbContext(DbContextOptions<MindBotDbContext> optionsBuilder) : base(
            optionsBuilder)
        {

        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.EnableSensitiveDataLogging();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserStateEntity>(entity =>
            {
                entity.Property(e => e.AnswersJson)
                      .HasDefaultValue("[]")
                      .ValueGeneratedOnAdd();
            });

            modelBuilder.UseIdentityColumns();

            base.OnModelCreating(modelBuilder);
        }
    }
}
