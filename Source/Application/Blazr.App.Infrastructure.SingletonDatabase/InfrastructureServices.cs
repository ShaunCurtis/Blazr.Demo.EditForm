/// ============================================================
/// Author: Shaun Curtis, Cold Elm Coders
/// License: Use And Donate
/// If you use it, donate something to a charity somewhere
/// ============================================================

using Blazr.App.Core;
using Microsoft.Extensions.DependencyInjection;

namespace Blazr.App.Infrastructure.SingletonDatabase;

public static class InfrastructureServices
{
    public static void AddAppSingletonDatabaseInfrastructureServices(this IServiceCollection services)
    {
        services.AddSingleton<WeatherDataProvider>();
        services.AddScoped<IDataBroker<WeatherForecastId,WeatherForecast>, WeatherDataBroker>();
    }
}
