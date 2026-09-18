using TransactionAggregation.Domain.Transactions;

namespace TransactionAggregation.Application.Abstractions;

public interface ITransactionRepository
{
    Task<int> AddRangeIgnoringDuplicatesAsync(
        IReadOnlyCollection<Transaction> transactions,
        CancellationToken cancellationToken);

    Task<PagedResult<Transaction>> SearchAsync(
        string customerId,
        DateTimeOffset? from,
        DateTimeOffset? to,
        TransactionCategory? category,
        int page,
        int pageSize,
        CancellationToken cancellationToken);

    Task<TransactionSummary> GetSummaryAsync(
        string customerId,
        DateTimeOffset from,
        DateTimeOffset to,
        CancellationToken cancellationToken);

    Task<IReadOnlyCollection<CategorySummary>> GetCategorySummaryAsync(
        string customerId,
        DateTimeOffset from,
        DateTimeOffset to,
        CancellationToken cancellationToken);
}

public sealed record PagedResult<T>(IReadOnlyCollection<T> Items, int Page, int PageSize, long Total);
public sealed record TransactionSummary(decimal Income, decimal Expenses, decimal Net, long TransactionCount, string Currency);
public sealed record CategorySummary(TransactionCategory Category, decimal Total, long TransactionCount);
