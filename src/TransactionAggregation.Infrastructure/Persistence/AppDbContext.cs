using Microsoft.EntityFrameworkCore;
using TransactionAggregation.Domain.Transactions;

namespace TransactionAggregation.Infrastructure.Persistence;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Transaction> Transactions => Set<Transaction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var transaction = modelBuilder.Entity<Transaction>();
        transaction.ToTable("transactions");
        transaction.HasKey(x => x.Id);
        transaction.Property(x => x.CustomerId).HasMaxLength(100).IsRequired();
        transaction.Property(x => x.AccountId).HasMaxLength(100).IsRequired();
        transaction.Property(x => x.ExternalTransactionId).HasMaxLength(255).IsRequired();
        transaction.Property(x => x.Source).HasConversion<string>().HasMaxLength(50).IsRequired();
        transaction.Property(x => x.Amount).HasPrecision(19, 4).IsRequired();
        transaction.Property(x => x.Currency).HasMaxLength(3).IsRequired();
        transaction.Property(x => x.Description).HasMaxLength(500).IsRequired();
        transaction.Property(x => x.Category).HasConversion<string>().HasMaxLength(50).IsRequired();
        transaction.Property(x => x.TransactionDate).IsRequired();
        transaction.HasIndex(x => new { x.Source, x.ExternalTransactionId }).IsUnique();
        transaction.HasIndex(x => new { x.CustomerId, x.TransactionDate });
        transaction.HasIndex(x => new { x.CustomerId, x.Category });
    }
}
