/// ============================================================
/// Author: Shaun Curtis, Cold Elm Coders
/// License: Use And Donate
/// If you use it, donate something to a charity somewhere
/// ============================================================

namespace Blazr.App.Data;

/// <summary>
/// This is the client version of the data broker.
/// It's used in the Wasm SPA and gets its data from the API 
/// </summary>
public class WeatherForecastAPIDataBroker : IWeatherForecastDataBroker
{
    private readonly HttpClient httpClient;

    public WeatherForecastAPIDataBroker(HttpClient httpClient)
        => this.httpClient = httpClient;

    public async ValueTask<bool> AddForecastAsync(DcoWeatherForecast record)
    {
        var response = await this.httpClient.PostAsJsonAsync<DcoWeatherForecast>($"/api/weatherforecast/add", record);
        var result = await response.Content.ReadFromJsonAsync<bool>();
        return result;
    }

    public async ValueTask<bool> UpdateForecastAsync(DcoWeatherForecast record)
    {
        var response = await this.httpClient.PostAsJsonAsync<DcoWeatherForecast>($"/api/weatherforecast/update", record);
        var result = await response.Content.ReadFromJsonAsync<bool>();
        return result;
    }

    public async ValueTask<bool> DeleteForecastAsync(Guid Id)
    {
        var response = await this.httpClient.PostAsJsonAsync<Guid>($"/api/weatherforecast/delete", Id);
        var result = await response.Content.ReadFromJsonAsync<bool>();
        return result;
    }

    public async ValueTask<DcoWeatherForecast> GetForecastAsync(Guid Id)
    {
        var response = await this.httpClient.PostAsJsonAsync<Guid>($"/api/weatherforecast/get", Id);
        var result = await response.Content.ReadFromJsonAsync<DcoWeatherForecast>();
        return result ?? new DcoWeatherForecast();
    }

    public async ValueTask<IEnumerable<DcoWeatherForecast>> GetWeatherForecastsAsync()
    {
        var list = await this.httpClient.GetFromJsonAsync<List<DcoWeatherForecast>>($"/api/weatherforecast/list");
        return list ?? Enumerable.Empty<DcoWeatherForecast>();
    }
}
