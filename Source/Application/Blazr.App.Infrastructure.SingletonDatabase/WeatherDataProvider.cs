/// ============================================================
/// Author: Shaun Curtis, Cold Elm Coders
/// License: Use And Donate
/// If you use it, donate something to a charity somewhere
/// ============================================================

using Blazr.App.Core;
using System.Diagnostics.CodeAnalysis;

namespace Blazr.App.Infrastructure.SingletonDatabase;

public class WeatherDataProvider
{
    public IEnumerable<WeatherForecast> WeatherForecasts => _weatherForecasts.OrderBy(item => item.Date);

    private List<WeatherForecast> _weatherForecasts;

    public async ValueTask<bool> AddWeatherForecastAsync(WeatherForecast record)
    {
        await Task.Yield();
        var exists = _weatherForecasts.Any(item => item.Id.Equals(record.Id));
        if (!exists)
            _weatherForecasts.Add(record);

        return !exists;
    }

    public async ValueTask<bool> DeleteWeatherForecastAsync(WeatherForecast record)
    {
        await Task.Yield();

        var existingRecord = _weatherForecasts.SingleOrDefault(item => item.Id.Equals(record.Id));

        if (existingRecord is not null)
            _weatherForecasts.Remove(existingRecord);

        return existingRecord is not null;
    }

    public async ValueTask<bool> UpdateWeatherForecastAsync(WeatherForecast record)
    {
        await Task.Yield();

        var existingRecord = _weatherForecasts.SingleOrDefault(item => item.Id.Equals(record.Id));
        if (existingRecord is not null)
        {
            _weatherForecasts.Remove(existingRecord);
            _weatherForecasts.Add(record);
        }

        return existingRecord is not null ;
    }

    public WeatherDataProvider()
    {
        var startDate = DateOnly.FromDateTime(DateTime.Now);
        var summaries = new[] { "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching" };
        _weatherForecasts = Enumerable.Range(1, 50).Select(index => new WeatherForecast
        {
            Id = new(Guid.NewGuid()),
            Date = startDate.AddDays(index),
            Temperature = new(Random.Shared.Next(-20, 55)),
            Summary = summaries[Random.Shared.Next(summaries.Length)]
        }).ToList();
    }
}
