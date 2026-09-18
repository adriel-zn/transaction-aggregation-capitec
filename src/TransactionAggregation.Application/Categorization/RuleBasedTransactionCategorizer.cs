using TransactionAggregation.Domain.Transactions;

namespace TransactionAggregation.Application.Categorization;

public sealed class RuleBasedTransactionCategorizer : ITransactionCategorizer
{
    private static readonly (TransactionCategory Category, string[] Keywords)[] Rules =
    [
        (TransactionCategory.Income, ["SALARY", "PAYROLL", "WAGE"]),
        (TransactionCategory.Groceries, ["CHECKERS", "SHOPRITE", "WOOLWORTHS FOOD", "PICK N PAY", "SPAR"]),
        (TransactionCategory.Transport, ["UBER", "BOLT", "GAUTRAIN", "MYCITI"]),
        (TransactionCategory.Fuel, ["ENGEN", "SHELL", "BP ", "SASOL"]),
        (TransactionCategory.Entertainment, ["NETFLIX", "SPOTIFY", "SHOWMAX", "STEAM"]),
        (TransactionCategory.Restaurants, ["NANDOS", "KFC", "MCDONALD", "RESTAURANT", "CAFE"]),
        (TransactionCategory.Utilities, ["ESKOM", "ELECTRICITY", "WATER", "TELKOM", "VODACOM", "MTN"]),
        (TransactionCategory.Healthcare, ["DIS-CHEM", "CLICKS", "PHARMACY", "MEDICAL"]),
        (TransactionCategory.Transfer, ["TRANSFER", "EFT", "PAYMENT TO"]),
        (TransactionCategory.Shopping, ["TAKEALOT", "AMAZON", "MR PRICE", "EDGARS"])
    ];

    public TransactionCategory Categorize(string description, decimal amount)
    {
        var normalized = description.ToUpperInvariant();
        foreach (var rule in Rules)
        {
            if (rule.Keywords.Any(normalized.Contains))
                return rule.Category;
        }

        if (amount > 0)
            return TransactionCategory.Income;

        return TransactionCategory.Other;
    }
}
