using TransactionAggregation.Domain.Transactions;

namespace TransactionAggregation.Application.Abstractions;

public interface ITransactionProvider
{
    TransactionSource Source { get; }

    Task<IReadOnlyCollection<ExternalTransaction>> GetTransactionsAsync(
        string customerId,
        DateTimeOffset from,
        DateTimeOffset to,
        CancellationToken cancellationToken);
}

public sealed record ExternalTransaction(
    string AccountId,
    string ExternalTransactionId,
    TransactionSource Source,
    decimal Amount,
    string Currency,
    string Description,
    DateTimeOffset TransactionDate);
