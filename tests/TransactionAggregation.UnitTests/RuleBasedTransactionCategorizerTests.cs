using FluentAssertions;
using TransactionAggregation.Application.Categorization;
using TransactionAggregation.Domain.Transactions;

namespace TransactionAggregation.UnitTests;

public sealed class RuleBasedTransactionCategorizerTests
{
    private readonly RuleBasedTransactionCategorizer _sut = new();

    [Theory]
    [InlineData("CHECKERS HYPER", -100, TransactionCategory.Groceries)]
    [InlineData("UBER TRIP", -100, TransactionCategory.Transport)]
    [InlineData("ENGEN FUEL STATION", -100, TransactionCategory.Fuel)]
    [InlineData("MONTHLY SALARY", 1000, TransactionCategory.Income)]
    [InlineData("UNKNOWN MERCHANT", -100, TransactionCategory.Other)]
    public void Categorize_ReturnsExpectedCategory(string description, decimal amount, TransactionCategory expected)
    {
        _sut.Categorize(description, amount).Should().Be(expected);
    }
}
