using Microsoft.EntityFrameworkCore;
using TransactionAggregation.Application.Categorization;
using TransactionAggregation.Application.Services;
using TransactionAggregation.Infrastructure;
using TransactionAggregation.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddProblemDetails();
builder.Services.AddHealthChecks().AddDbContextCheck<AppDbContext>();
builder.Services.AddScoped<ITransactionCategorizer, RuleBasedTransactionCategorizer>();
builder.Services.AddScoped<TransactionAggregationService>();
builder.Services.AddScoped<TransactionQueryService>();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

app.UseExceptionHandler();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();
app.MapHealthChecks("/health");

await using (var scope = app.Services.CreateAsyncScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
}

app.Run();

public partial class Program;
