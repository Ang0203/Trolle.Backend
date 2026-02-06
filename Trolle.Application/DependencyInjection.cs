using Microsoft.Extensions.DependencyInjection;
using Trolle.Application.Interfaces;
using Trolle.Application.Services;

namespace Trolle.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IBoardService, BoardService>();
        services.AddScoped<IColumnService, ColumnService>();
        services.AddScoped<ICardService, CardService>();
        services.AddScoped<ILabelService, LabelService>();
        return services;
    }
}