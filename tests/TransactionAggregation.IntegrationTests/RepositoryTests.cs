using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;
using TransactionAggregation.Domain.Transactions;
using TransactionAggregation.Infrastructure.Persistence;

namespace TransactionAggregation.IntegrationTests;

public sealed class RepositoryTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder().WithImage("postgres:17-alpine").Build();

    public Task InitializeAsync() => _postgres.StartAsync();
    public Task DisposeAsync() => _postgres.DisposeAsync().AsTask();

    [Fact]
    public async Task AddRangeIgnoringDuplicatesAsync_DeduplicatesBySourceAndExternalId()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(_postgres.GetConnectionString())
            .Options;

        await using var db = new AppDbContext(options);
        await db.Database.MigrateAsync();
        var repository = new EfTransactionRepository(db);

        var first = Transaction.Create("cust-1", "acc-1", "ext-1", TransactionSource.BankA, -10m, "ZAR", "CHECKERS", TransactionCategory.Groceries, DateTimeOffset.UtcNow);
        var duplicate = Transaction.Create("cust-1", "acc-1", "ext-1", TransactionSource.BankA, -10m, "ZAR", "CHECKERS", TransactionCategory.Groceries, DateTimeOffset.UtcNow);

        var created1 = await repository.AddRangeIgnoringDuplicatesAsync([first], CancellationToken.None);
        var created2 = await repository.AddRangeIgnoringDuplicatesAsync([duplicate], CancellationToken.None);

        created1.Should().Be(1);
        created2.Should().Be(0);
        (await db.Transactions.CountAsync()).Should().Be(1);
    }
}
