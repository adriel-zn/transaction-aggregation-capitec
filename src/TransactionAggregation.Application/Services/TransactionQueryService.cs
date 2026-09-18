using TransactionAggregation.Application.Abstractions;
using TransactionAggregation.Domain.Transactions;

namespace TransactionAggregation.Application.Services;

public sealed class TransactionQueryService(ITransactionRepository repository)
{
    public Task<PagedResult<Transaction>> SearchAsync(
        string customerId,
        DateTimeOffset? from,
        DateTimeOffset? to,
        TransactionCategory? category,
        int page,
        int pageSize,
        CancellationToken cancellationToken) =>
        repository.SearchAsync(customerId, from, to, category, page, pageSize, cancellationToken);

    public Task<TransactionSummary> GetSummaryAsync(
        string customerId,
        DateTimeOffset from,
        DateTimeOffset to,
        CancellationToken cancellationToken) =>
        repository.GetSummaryAsync(customerId, from, to, cancellationToken);

    public Task<IReadOnlyCollection<CategorySummary>> GetCategorySummaryAsync(
        string customerId,
        DateTimeOffset from,
        DateTimeOffset to,
        CancellationToken cancellationToken) =>
        repository.GetCategorySummaryAsync(customerId, from, to, cancellationToken);
}
