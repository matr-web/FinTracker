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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserEntity>().HasMany(u => u.History)
                 .WithOne(h => h.User)
                 .HasForeignKey(h => h.UserId);

        modelBuilder.Entity<UserEntity>().HasMany(u => u.Holdings)
         .WithOne(h => h.User)
         .HasForeignKey(h => h.UserId);

        modelBuilder.Entity<HistoryEntity>()
         .Property(p => p.Date)
         .HasColumnType("date");

        modelBuilder.Entity<HistoryEntity>()
         .Property(p => p.PricePerUnit)
         .HasColumnType("decimal(18,2)");

        modelBuilder.Entity<HistoryEntity>()
         .Property(p => p.CurrencyPrice)
         .HasColumnType("decimal(18,2)");

        modelBuilder.Entity<HistoryEntity>()
         .Property(p => p.Profit)
         .HasColumnType("decimal(18,2)");

        modelBuilder.Entity<HoldingEntity>()
         .Property(p => p.Value)
         .HasColumnType("decimal(18,2)");
    }
}
