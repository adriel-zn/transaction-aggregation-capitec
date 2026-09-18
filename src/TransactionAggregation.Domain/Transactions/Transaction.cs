namespace TransactionAggregation.Domain.Transactions;

public sealed class Transaction
{
    private Transaction() { }

    private Transaction(
        Guid id,
        string customerId,
        string accountId,
        string externalTransactionId,
        TransactionSource source,
        decimal amount,
        string currency,
        string description,
        TransactionCategory category,
        DateTimeOffset transactionDate)
    {
        Id = id;
        CustomerId = customerId;
        AccountId = accountId;
        ExternalTransactionId = externalTransactionId;
        Source = source;
        Amount = amount;
        Currency = currency;
        Description = description;
        Category = category;
        TransactionDate = transactionDate;
    }

    public Guid Id { get; private set; }
    public string CustomerId { get; private set; } = string.Empty;
    public string AccountId { get; private set; } = string.Empty;
    public string ExternalTransactionId { get; private set; } = string.Empty;
    public TransactionSource Source { get; private set; }
    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public TransactionCategory Category { get; private set; }
    public DateTimeOffset TransactionDate { get; private set; }

    public static Transaction Create(
        string customerId,
        string accountId,
        string externalTransactionId,
        TransactionSource source,
        decimal amount,
        string currency,
        string description,
        TransactionCategory category,
        DateTimeOffset transactionDate)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(customerId);
        ArgumentException.ThrowIfNullOrWhiteSpace(accountId);
        ArgumentException.ThrowIfNullOrWhiteSpace(externalTransactionId);
        ArgumentException.ThrowIfNullOrWhiteSpace(currency);
        ArgumentException.ThrowIfNullOrWhiteSpace(description);

        return new Transaction(
            Guid.NewGuid(), customerId, accountId, externalTransactionId,
            source, amount, currency.ToUpperInvariant(), description.Trim(), category, transactionDate);
    }
}
