using TransactionAggregation.Application.Abstractions;
using TransactionAggregation.Application.Categorization;
using TransactionAggregation.Domain.Transactions;

namespace TransactionAggregation.Application.Services;

public sealed class TransactionAggregationService(
    IEnumerable<ITransactionProvider> providers,
    ITransactionCategorizer categorizer,
    ITransactionRepository repository)
{
    public async Task<AggregationResult> AggregateAsync(
        string customerId,
        DateTimeOffset from,
        DateTimeOffset to,
        CancellationToken cancellationToken)
    {
        if (from > to)
            throw new ArgumentException("'from' must be before or equal to 'to'.");

        var providerList = providers.ToArray();
        var successes = new List<ExternalTransaction>();
        var failures = new List<ProviderFailure>();

        var tasks = providerList.Select(async provider =>
        {
            try
            {
                var items = await provider.GetTransactionsAsync(customerId, from, to, cancellationToken);
                return (provider.Source, Items: items, Error: (Exception?)null);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                return (provider.Source, Items: (IReadOnlyCollection<ExternalTransaction>)Array.Empty<ExternalTransaction>(), Error: ex);
            }
        });

        foreach (var result in await Task.WhenAll(tasks))
        {
            if (result.Error is null)
                successes.AddRange(result.Items);
            else
                failures.Add(new ProviderFailure(result.Source, "UPSTREAM_ERROR", result.Error.Message));
        }

        var transactions = successes
            .Where(x => x.TransactionDate >= from && x.TransactionDate <= to)
            .Select(x => Transaction.Create(
                customerId,
                x.AccountId,
                x.ExternalTransactionId,
                x.Source,
                x.Amount,
                x.Currency,
                x.Description,
                categorizer.Categorize(x.Description, x.Amount),
                x.TransactionDate))
            .ToArray();

        var created = await repository.AddRangeIgnoringDuplicatesAsync(transactions, cancellationToken);

        return new AggregationResult(
            customerId,
            providerList.Length,
            providerList.Length - failures.Count,
            failures.Count,
            transactions.Length,
            created,
            transactions.Length - created,
            failures);
    }
}

public sealed record AggregationResult(
    string CustomerId,
    int SourcesRequested,
    int SourcesSucceeded,
    int SourcesFailed,
    int TransactionsReceived,
    int TransactionsCreated,
    int DuplicatesIgnored,
    IReadOnlyCollection<ProviderFailure> Failures);

public sealed record ProviderFailure(TransactionSource Source, string Code, string Message);
