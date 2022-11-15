﻿/// ============================================================
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

    public async ValueTask<CommandResult> AddForecastAsync(WeatherForecast record)
    {
        var response = await this.httpClient.PostAsJsonAsync<WeatherForecast>($"/api/weatherforecast/add", record);
        var result = await response.Content.ReadFromJsonAsync<CommandResult>();
        return result ?? CommandResult.Failure("API problem");
    }

    public async ValueTask<CommandResult> UpdateForecastAsync(WeatherForecast record)
    {
        var response = await this.httpClient.PostAsJsonAsync<WeatherForecast>($"/api/weatherforecast/update", record);
        var result = await response.Content.ReadFromJsonAsync<CommandResult>();
        return result ?? CommandResult.Failure("API problem");
    }

    public async ValueTask<CommandResult> DeleteForecastAsync(Guid Id)
    {
        var response = await this.httpClient.PostAsJsonAsync<Guid>($"/api/weatherforecast/delete", Id);
        var result = await response.Content.ReadFromJsonAsync<CommandResult>();
        return result ?? CommandResult.Failure("API problem");
    }

    public async ValueTask<ItemQueryResult<WeatherForecast>> GetForecastAsync(Guid Id)
    {
        var response = await this.httpClient.PostAsJsonAsync<Guid>($"/api/weatherforecast/get", Id);
        var result = await response.Content.ReadFromJsonAsync<ItemQueryResult<WeatherForecast>>();
        return result ?? ItemQueryResult<WeatherForecast>.Failure("API problem");
    }

    public async ValueTask<ListQueryResult<WeatherForecast>> GetWeatherForecastsAsync()
    {
        var response = await this.httpClient.GetFromJsonAsync<ListQueryResult<WeatherForecast>>($"/api/weatherforecast/list");
        return response ?? ListQueryResult<WeatherForecast>.Failure("API problem");
    }
}
