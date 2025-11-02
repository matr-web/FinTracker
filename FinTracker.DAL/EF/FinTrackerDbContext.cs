using FinTracker.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace FinTracker.DAL.EF;

public class FinTrackerDbContext : DbContext
{
    public FinTrackerDbContext(DbContextOptions<FinTrackerDbContext> options) : base(options)
    {
    }

    public DbSet<UserEntity> Users { get; set; }
    public DbSet<HistoryEntity> Histories { get; set; }
    public DbSet<HoldingEntity> Holdings { get; set; }
    public DbSet<DebtEntity> Debts { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserEntity>().HasMany(u => u.History)
                 .WithOne(h => h.User)
                 .HasForeignKey(h => h.UserId);

        modelBuilder.Entity<UserEntity>().HasMany(u => u.Holdings)
         .WithOne(h => h.User)
         .HasForeignKey(h => h.UserId);

        modelBuilder.Entity<UserEntity>().HasMany(u => u.Debts)
            .WithOne(h => h.User)
            .HasForeignKey(h => h.UserId);

        modelBuilder.Entity<HistoryEntity>()
         .Property(h => h.Date)
         .HasColumnType("date");

        modelBuilder.Entity<HistoryEntity>()
         .Property(h => h.PricePerUnit)
         .HasColumnType("decimal(18,2)");

        modelBuilder.Entity<HistoryEntity>()
         .Property(h => h.CurrencyPrice)
         .HasColumnType("decimal(18,2)");

        modelBuilder.Entity<HistoryEntity>()
         .Property(h => h.Profit)
         .HasColumnType("decimal(18,2)");

        modelBuilder.Entity<HoldingEntity>()
         .Property(h => h.Value)
         .HasColumnType("decimal(18,2)");

        modelBuilder.Entity<DebtEntity>()
         .Property(d => d.InterestRateProcentage)
         .HasColumnType("decimal(5, 4)");

        modelBuilder.Entity<DebtEntity>()
          .Property(d => d.Amount)
          .HasColumnType("decimal(18, 2)");

        modelBuilder.Entity<DebtEntity>()
          .Property(d => d.InstallmentAmount)
          .HasColumnType("decimal(18, 2)");
    }
}
