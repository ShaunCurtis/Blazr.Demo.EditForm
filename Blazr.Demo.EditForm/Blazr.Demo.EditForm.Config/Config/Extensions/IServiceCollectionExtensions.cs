/// ============================================================
/// Author: Shaun Curtis, Cold Elm Coders
/// License: Use And Donate
/// If you use it, donate something to a charity somewhere
/// ============================================================

namespace Blazr.Demo.EditForm.Config
{
    public static class IServiceCollectionExtensions
    {
        public static void AddAppBlazorServerServices(this IServiceCollection services)
        {
            services.AddSingleton<WeatherForecastDataStore>();
            services.AddSingleton<IWeatherForecastDataBroker, WeatherForecastServerDataBroker>();
            services.AddScoped<WeatherForecastsViewService>();
            services.AddScoped<WeatherForecastViewService>();
            //TODO - No longer needed
            //services.AddBlazrNavigationLockerServerServices();

        }

        public static void AddAppBlazorWASMServices(this IServiceCollection services)
        {
            services.AddScoped<IWeatherForecastDataBroker, WeatherForecastAPIDataBroker>();
            services.AddScoped<WeatherForecastViewService>();
            services.AddScoped<WeatherForecastsViewService>();
            services.AddBlazrNavigationLockerWASMServices();
        }

        public static void AddAppWASMServerServices(this IServiceCollection services)
        {
            services.AddSingleton<WeatherForecastDataStore>();
            services.AddSingleton<IWeatherForecastDataBroker, WeatherForecastServerDataBroker>();
        }
    }
}
