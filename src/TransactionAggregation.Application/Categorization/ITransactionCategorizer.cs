using TransactionAggregation.Domain.Transactions;

namespace TransactionAggregation.Application.Categorization;

public interface ITransactionCategorizer
{
    TransactionCategory Categorize(string description, decimal amount);
}
