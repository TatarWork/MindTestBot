using Microsoft.EntityFrameworkCore;
using MindTestBot.Entities;

namespace MindTestBot
{
    public class AppDbContext : DbContext
    {
        public DbSet<UserTestState> UserTestStates { get; set; }

        public DbSet<TestQuestion> TestQuestions { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> optionsBuilder) : base(
            optionsBuilder)
        {

        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.EnableSensitiveDataLogging();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserTestState>(entity =>
            {
                entity.Property(e => e.AnswersJson)
                      .HasDefaultValue("[]")
                      .ValueGeneratedOnAdd();

                entity.Property(e => e.CreatedAt)
                      .HasDefaultValueSql("NOW()");

                entity.Property(e => e.UpdatedAt)
                      .HasDefaultValueSql("NOW()")
                      .ValueGeneratedOnAddOrUpdate();
            });

            modelBuilder.Entity<TestQuestion>(entity =>
            {
                entity.HasIndex(e => e.Order)
                      .IsUnique();

                entity.Property(e => e.OptionsJson)
                      .HasDefaultValue("{}");
            });

            modelBuilder.UseIdentityColumns();

            base.OnModelCreating(modelBuilder);
        }
    }
}
