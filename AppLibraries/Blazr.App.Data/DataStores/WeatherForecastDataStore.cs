/// ============================================================
/// Author: Shaun Curtis, Cold Elm Coders
/// License: Use And Donate
/// If you use it, donate something to a charity somewhere
/// ============================================================

namespace Blazr.App.Data;

public class WeatherForecastDataStore
{
    private int _recordsToGet = 5;
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    private List<DboWeatherForecast> _records;

    public WeatherForecastDataStore()
        =>
        _records = GetForecasts();

    private List<DboWeatherForecast> GetForecasts()
    {
        var rng = new Random();
        return Enumerable.Range(1, _recordsToGet).Select(index => new DboWeatherForecast
        {
            Id = Guid.NewGuid(),
            Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            TemperatureC = rng.Next(-20, 55),
            Summary = Summaries[rng.Next(Summaries.Length)]
        }).ToList();
    }

    public ValueTask<bool> UpdateForecastAsync(DroWeatherForecast weatherForecast)
    {
        var record = _records.FirstOrDefault(item => item.Id == weatherForecast.Id);
        if (record is not null)
            _records.Remove(record);
        _records.Add(DboWeatherForecast.FromDto(weatherForecast));
        _records = _records.OrderBy(item => item.Date).ToList();
        return ValueTask.FromResult(true);
    }

    public ValueTask<bool> AddForecastAsync(DroWeatherForecast weatherForecast)
    {
        var record = DboWeatherForecast.FromDto(weatherForecast);
        _records.Add(record);
        _records = _records.OrderBy(item => item.Date).ToList();
        return ValueTask.FromResult(true);
    }

    public ValueTask<DroWeatherForecast> GetForecastAsync(Guid Id)
    {
        var record = _records.FirstOrDefault(item => item.Id == Id);
        return ValueTask.FromResult(record?.ToDto() ?? new DroWeatherForecast());
    }

    public ValueTask<bool> DeleteForecastAsync(Guid Id)
    {
        var deleted = false;
        var record = _records.FirstOrDefault(item => item.Id == Id);
        if (record != null)
        {
            _records.Remove(record);
            deleted = true;
        }
        return ValueTask.FromResult(deleted);
    }

    public ValueTask<List<DroWeatherForecast>> GetWeatherForecastsAsync()
    {
        var list = new List<DroWeatherForecast>();
        _records
            .ForEach(item => list.Add(item.ToDto()));
        return ValueTask.FromResult(list);
    }

    public void OverrideWeatherForecastDateSet(List<DroWeatherForecast> list)
    {
        _records.Clear();
        list.ForEach(item => _records.Add(DboWeatherForecast.FromDto(item)));
    }

    public static List<DroWeatherForecast> CreateTestForecasts(int count)
    {
        var rng = new Random();
        return Enumerable.Range(1, count).Select(index => new DroWeatherForecast
        {
            Id = Guid.NewGuid(),
            Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            TemperatureC = rng.Next(-20, 55),
            Summary = Summaries[rng.Next(Summaries.Length)]
        }).ToList();
    }
}
