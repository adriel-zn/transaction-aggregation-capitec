using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TransactionAggregation.Application.Abstractions;
using TransactionAggregation.Infrastructure.Persistence;
using TransactionAggregation.Infrastructure.Providers;

namespace TransactionAggregation.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Postgres")));

        services.AddScoped<ITransactionRepository, EfTransactionRepository>();
        services.AddScoped<ITransactionProvider, BankAProvider>();
        services.AddScoped<ITransactionProvider, BankBProvider>();
        services.AddScoped<ITransactionProvider, BankCProvider>();
        return services;
    }
}
