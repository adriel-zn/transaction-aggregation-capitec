using Microsoft.EntityFrameworkCore;
using Npgsql;
using TransactionAggregation.Application.Abstractions;
using TransactionAggregation.Domain.Transactions;

namespace TransactionAggregation.Infrastructure.Persistence;

public sealed class EfTransactionRepository(AppDbContext dbContext) : ITransactionRepository
{
    public async Task<int> AddRangeIgnoringDuplicatesAsync(
        IReadOnlyCollection<Transaction> transactions,
        CancellationToken cancellationToken)
    {
        var created = 0;
        foreach (var transaction in transactions)
        {
            dbContext.Transactions.Add(transaction);
            try
            {
                await dbContext.SaveChangesAsync(cancellationToken);
                created++;
            }
            catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
            {
                dbContext.Entry(transaction).State = EntityState.Detached;
            }
        }

        return created;
    }

    public async Task<PagedResult<Transaction>> SearchAsync(
        string customerId,
        DateTimeOffset? from,
        DateTimeOffset? to,
        TransactionCategory? category,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var query = dbContext.Transactions.AsNoTracking().Where(x => x.CustomerId == customerId);
        if (from is not null) query = query.Where(x => x.TransactionDate >= from);
        if (to is not null) query = query.Where(x => x.TransactionDate <= to);
        if (category is not null) query = query.Where(x => x.Category == category);

        var total = await query.LongCountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(x => x.TransactionDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Transaction>(items, page, pageSize, total);
    }

    public async Task<TransactionSummary> GetSummaryAsync(
        string customerId,
        DateTimeOffset from,
        DateTimeOffset to,
        CancellationToken cancellationToken)
    {
        var query = dbContext.Transactions.AsNoTracking()
            .Where(x => x.CustomerId == customerId && x.TransactionDate >= from && x.TransactionDate <= to);

        var count = await query.LongCountAsync(cancellationToken);
        var income = await query.Where(x => x.Amount > 0).SumAsync(x => (decimal?)x.Amount, cancellationToken) ?? 0m;
        var expenseSigned = await query.Where(x => x.Amount < 0).SumAsync(x => (decimal?)x.Amount, cancellationToken) ?? 0m;
        var expenses = Math.Abs(expenseSigned);
        var currency = await query.Select(x => x.Currency).FirstOrDefaultAsync(cancellationToken) ?? "ZAR";

        return new TransactionSummary(income, expenses, income - expenses, count, currency);
    }

    public async Task<IReadOnlyCollection<CategorySummary>> GetCategorySummaryAsync(
        string customerId,
        DateTimeOffset from,
        DateTimeOffset to,
        CancellationToken cancellationToken)
    {
        return await dbContext.Transactions.AsNoTracking()
            .Where(x => x.CustomerId == customerId && x.TransactionDate >= from && x.TransactionDate <= to && x.Amount < 0)
            .GroupBy(x => x.Category)
            .Select(g => new CategorySummary(g.Key, Math.Abs(g.Sum(x => x.Amount)), g.LongCount()))
            .OrderByDescending(x => x.Total)
            .ToListAsync(cancellationToken);
    }
}
