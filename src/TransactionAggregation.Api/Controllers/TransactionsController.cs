using Microsoft.AspNetCore.Mvc;
using TransactionAggregation.Application.Services;
using TransactionAggregation.Domain.Transactions;

namespace TransactionAggregation.Api.Controllers;

[ApiController]
[Route("api/v1/customers/{customerId}")]
public sealed class TransactionsController(
    TransactionAggregationService aggregationService,
    TransactionQueryService queryService) : ControllerBase
{
    [HttpPost("transactions/import")]
    [ProducesResponseType<AggregationResult>(StatusCodes.Status200OK)]
    public async Task<ActionResult<AggregationResult>> Import(
        string customerId,
        [FromBody] ImportTransactionsRequest request,
        CancellationToken cancellationToken)
    {
        if (request.From > request.To)
            return ValidationProblem(new ValidationProblemDetails(
                new Dictionary<string, string[]> { ["dateRange"] = ["'from' must be before or equal to 'to'."] }));

        var result = await aggregationService.AggregateAsync(customerId, request.From, request.To, cancellationToken);
        return Ok(result);
    }

    [HttpGet("transactions")]
    public async Task<IActionResult> GetTransactions(
        string customerId,
        [FromQuery] DateTimeOffset? from,
        [FromQuery] DateTimeOffset? to,
        [FromQuery] TransactionCategory? category,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        if (page < 1 || pageSize is < 1 or > 200)
            return ValidationProblem(new ValidationProblemDetails(
                new Dictionary<string, string[]> { ["pagination"] = ["page must be >= 1 and pageSize must be between 1 and 200."] }));

        var result = await queryService.SearchAsync(customerId, from, to, category, page, pageSize, cancellationToken);
        return Ok(result);
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary(
        string customerId,
        [FromQuery] DateTimeOffset from,
        [FromQuery] DateTimeOffset to,
        CancellationToken cancellationToken)
    {
        if (from > to)
            return ValidationProblem(new ValidationProblemDetails(
                new Dictionary<string, string[]> { ["dateRange"] = ["'from' must be before or equal to 'to'."] }));

        return Ok(await queryService.GetSummaryAsync(customerId, from, to, cancellationToken));
    }

    [HttpGet("categories")]
    public async Task<IActionResult> GetCategorySummary(
        string customerId,
        [FromQuery] DateTimeOffset from,
        [FromQuery] DateTimeOffset to,
        CancellationToken cancellationToken)
    {
        if (from > to)
            return ValidationProblem(new ValidationProblemDetails(
                new Dictionary<string, string[]> { ["dateRange"] = ["'from' must be before or equal to 'to'."] }));

        return Ok(await queryService.GetCategorySummaryAsync(customerId, from, to, cancellationToken));
    }
}

public sealed record ImportTransactionsRequest(DateTimeOffset From, DateTimeOffset To);
