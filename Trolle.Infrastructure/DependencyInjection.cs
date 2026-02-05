using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Trolle.Infrastructure.Options;
using Trolle.Infrastructure.Persistence;
using Trolle.Application.Interfaces.Persistence;
using Trolle.Infrastructure.Repositories;

namespace Trolle.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddDbContext<TrolleDbContext>((sp, options) =>
        {
            var dbOptions = sp
                .GetRequiredService<IOptions<DatabaseOptions>>()
                .Value;

            options.UseNpgsql(
                dbOptions.ConnectionString,
                b => b.MigrationsAssembly(typeof(TrolleDbContext).Assembly.FullName)
                      .UseQuerySplittingBehavior(QuerySplittingBehavior.SingleQuery)
            );
        });
        
        services.AddScoped<IBoardRepository, BoardRepository>();
        services.AddScoped<ICardRepository, CardRepository>();
        services.AddScoped<ILabelRepository, LabelRepository>();
        
        return services;
    }
}
