using TransactionAggregation.Application.Abstractions;
using TransactionAggregation.Domain.Transactions;

namespace TransactionAggregation.Infrastructure.Providers;

public sealed class BankAProvider : ITransactionProvider
{
    public TransactionSource Source => TransactionSource.BankA;

    public Task<IReadOnlyCollection<ExternalTransaction>> GetTransactionsAsync(
        string customerId, DateTimeOffset from, DateTimeOffset to, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        BankATransaction[] response =
        [
            new("A-1001", "ACC-001", "-349.50", "ZAR", "CHECKERS HYPER", from.AddDays(1)),
            new("A-1002", "ACC-001", "-179.00", "ZAR", "UBER TRIP", from.AddDays(2)),
            new("A-1003", "ACC-001", "48000.00", "ZAR", "MONTHLY SALARY", from.AddDays(3))
        ];

        IReadOnlyCollection<ExternalTransaction> result = response.Select(x => new ExternalTransaction(
            x.AccountNumber,
            x.TransactionId,
            Source,
            decimal.Parse(x.Amount, System.Globalization.CultureInfo.InvariantCulture),
            x.Currency,
            x.Description,
            x.Timestamp)).ToArray();

        return Task.FromResult(result);
    }

    private sealed record BankATransaction(
        string TransactionId,
        string AccountNumber,
        string Amount,
        string Currency,
        string Description,
        DateTimeOffset Timestamp);
}

public sealed class BankBProvider : ITransactionProvider
{
    public TransactionSource Source => TransactionSource.BankB;

    public Task<IReadOnlyCollection<ExternalTransaction>> GetTransactionsAsync(
        string customerId, DateTimeOffset from, DateTimeOffset to, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        BankBTransaction[] response =
        [
            new("B-2001", "ACC-001", 899.00m, null, "ENGEN FUEL STATION", "ZAR", from.AddDays(4)),
            new("B-2002", "ACC-001", 199.00m, null, "NETFLIX.COM", "ZAR", from.AddDays(5)),
            new("B-2003", "ACC-001", 599.99m, null, "TAKEALOT ONLINE", "ZAR", from.AddDays(6))
        ];

        IReadOnlyCollection<ExternalTransaction> result = response.Select(x => new ExternalTransaction(
            x.Account,
            x.Id,
            Source,
            x.Credit ?? -(x.Debit ?? 0m),
            x.CurrencyCode,
            x.Merchant,
            x.BookingDate)).ToArray();

        return Task.FromResult(result);
    }

    private sealed record BankBTransaction(
        string Id,
        string Account,
        decimal? Debit,
        decimal? Credit,
        string Merchant,
        string CurrencyCode,
        DateTimeOffset BookingDate);
}

public sealed class BankCProvider : ITransactionProvider
{
    public TransactionSource Source => TransactionSource.BankC;

    public Task<IReadOnlyCollection<ExternalTransaction>> GetTransactionsAsync(
        string customerId, DateTimeOffset from, DateTimeOffset to, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        BankCTransaction[] response =
        [
            new("C-3001", "ACC-002", new Money(1250m, "ZAR"), "WOOLWORTHS FOOD", "DEBIT", from.AddDays(7)),
            new("C-3002", "ACC-002", new Money(450m, "ZAR"), "NANDOS", "DEBIT", from.AddDays(8)),
            new("C-3003", "ACC-002", new Money(850m, "ZAR"), "ESKOM ELECTRICITY", "DEBIT", from.AddDays(9))
        ];

        IReadOnlyCollection<ExternalTransaction> result = response.Select(x => new ExternalTransaction(
            x.CustomerAccount,
            x.Reference,
            Source,
            x.Type.Equals("CREDIT", StringComparison.OrdinalIgnoreCase) ? x.Value.Amount : -x.Value.Amount,
            x.Value.Currency,
            x.Narrative,
            x.BookedAt)).ToArray();

        return Task.FromResult(result);
    }

    private sealed record Money(decimal Amount, string Currency);
    private sealed record BankCTransaction(
        string Reference,
        string CustomerAccount,
        Money Value,
        string Narrative,
        string Type,
        DateTimeOffset BookedAt);
}
