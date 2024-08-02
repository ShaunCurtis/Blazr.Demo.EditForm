/// ============================================================
/// Author: Shaun Curtis, Cold Elm Coders
/// License: Use And Donate
/// If you use it, donate something to a charity somewhere
/// ============================================================

using Blazr.App.Core;

namespace Blazr.App.Infrastructure.SingletonDatabase;

public class WeatherDataProvider
{
    public List<WeatherForecast> WeatherForecasts => _weatherForecasts.OrderBy(item => item.Date).ToList();

    private List<WeatherForecast> _weatherForecasts;

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
