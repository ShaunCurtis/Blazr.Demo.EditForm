/// ============================================================
/// Author: Shaun Curtis, Cold Elm Coders
/// License: Use And Donate
/// If you use it, donate something to a charity somewhere
/// ============================================================


namespace Blazr.App.Data;

/// <summary>
/// This is the server version of the data broker.
/// It's used in the Server SPA and in the API web server for the WASM SPA 
/// </summary>
public class WeatherForecastServerDataBroker : IWeatherForecastDataBroker
{
    private readonly WeatherForecastDataStore weatherForecastDataStore;

    public WeatherForecastServerDataBroker(WeatherForecastDataStore weatherForecastDataStore)
        => this.weatherForecastDataStore = weatherForecastDataStore;
    public async ValueTask<DroWeatherForecast> GetForecastAsync(Guid Id)
        => await this.weatherForecastDataStore!.GetForecastAsync(Id);

    public async ValueTask<bool> AddForecastAsync(DroWeatherForecast record)
        => await this.weatherForecastDataStore!.AddForecastAsync(record);

    public async ValueTask<bool> DeleteForecastAsync(Guid Id)
        => await this.weatherForecastDataStore!.DeleteForecastAsync(Id);

    public async ValueTask<bool> UpdateForecastAsync(DroWeatherForecast record)
        => await this.weatherForecastDataStore!.UpdateForecastAsync(record);

    public async ValueTask<IEnumerable<DroWeatherForecast>> GetWeatherForecastsAsync()
        => await this.weatherForecastDataStore!.GetWeatherForecastsAsync();
}
