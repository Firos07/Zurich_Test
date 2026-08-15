using Claims.Application.Abstractions;
using Claims.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Claims.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("ClaimsDb")
            ?? throw new InvalidOperationException("La connection string 'ClaimsDb' no está configurada.");

        services.AddDbContext<ClaimsDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddScoped<IClaimRepository, ClaimsRepository>();

        return services;
    }
}