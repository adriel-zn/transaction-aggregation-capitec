using FluentAssertions;
using TransactionAggregation.Application.Abstractions;
using TransactionAggregation.Application.Categorization;
using TransactionAggregation.Application.Services;
using TransactionAggregation.Domain.Transactions;

namespace TransactionAggregation.UnitTests;

public sealed class TransactionAggregationServiceTests
{
    [Fact]
    public async Task AggregateAsync_WhenOneProviderFails_PersistsSuccessfulTransactionsAndReportsPartialFailure()
    {
        var from = new DateTimeOffset(2026, 9, 1, 0, 0, 0, TimeSpan.Zero);
        var to = from.AddDays(30);
        ITransactionProvider[] providers =
        [
            new SuccessfulProvider(from.AddDays(1)),
            new FailingProvider()
        ];
        var repository = new FakeRepository();
        var service = new TransactionAggregationService(providers, new RuleBasedTransactionCategorizer(), repository);

        var result = await service.AggregateAsync("cust-1", from, to, CancellationToken.None);

        result.SourcesRequested.Should().Be(2);
        result.SourcesSucceeded.Should().Be(1);
        result.SourcesFailed.Should().Be(1);
        result.TransactionsCreated.Should().Be(1);
        repository.Saved.Should().ContainSingle();
        repository.Saved.Single().Category.Should().Be(TransactionCategory.Groceries);
    }

    private sealed class SuccessfulProvider(DateTimeOffset date) : ITransactionProvider
    {
        public TransactionSource Source => TransactionSource.BankA;

        public Task<IReadOnlyCollection<ExternalTransaction>> GetTransactionsAsync(
            string customerId, DateTimeOffset from, DateTimeOffset to, CancellationToken cancellationToken)
        {
            IReadOnlyCollection<ExternalTransaction> result =
            [new("acc-1", "ext-1", Source, -50m, "ZAR", "CHECKERS", date)];
            return Task.FromResult(result);
        }
    }

    private sealed class FailingProvider : ITransactionProvider
    {
        public TransactionSource Source => TransactionSource.BankB;

        public Task<IReadOnlyCollection<ExternalTransaction>> GetTransactionsAsync(
            string customerId, DateTimeOffset from, DateTimeOffset to, CancellationToken cancellationToken) =>
            throw new HttpRequestException("Provider unavailable");
    }

    private sealed class FakeRepository : ITransactionRepository
    {
        public List<Transaction> Saved { get; } = [];

        public Task<int> AddRangeIgnoringDuplicatesAsync(IReadOnlyCollection<Transaction> transactions, CancellationToken cancellationToken)
        {
            Saved.AddRange(transactions);
            return Task.FromResult(transactions.Count);
        }

        public Task<PagedResult<Transaction>> SearchAsync(string customerId, DateTimeOffset? from, DateTimeOffset? to, TransactionCategory? category, int page, int pageSize, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<TransactionSummary> GetSummaryAsync(string customerId, DateTimeOffset from, DateTimeOffset to, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<IReadOnlyCollection<CategorySummary>> GetCategorySummaryAsync(string customerId, DateTimeOffset from, DateTimeOffset to, CancellationToken cancellationToken) => throw new NotSupportedException();
    }
}
